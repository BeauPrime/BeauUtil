/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMTextUI.cs
 * Purpose: Debug menu display code.
 */

using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeauUtil.Debugger
{
    public class DMMenuUI : MonoBehaviour
    {
        private struct MenuStackItem
        {
            public DMInfo Menu;
            public int PageIndex;
            public int ArrowIndex;

            public MenuStackItem(DMInfo inMenu, int inPageIndex, int inArrowIndex)
            {
                Menu = inMenu;
                PageIndex = inPageIndex;
                ArrowIndex = inArrowIndex;
            }

            static public readonly MenuStackItem Empty = new MenuStackItem(null, 0, -1);
        }

        public enum NavigationCommand
        {
            None,
            NextPage,
            PrevPage,
            Back,
            MoveArrowDown,
            MoveArrowUp,
            SelectArrow,
            ClearArrow,
            DecreaseSlider,
            IncreaseSlider,
        }

        #region Inspector

        [SerializeField] private DMHeaderUI m_Header = null;
        [SerializeField] private DMPageUI m_Page = null;

        [Header("Prefabs")]
        [SerializeField] private RectTransform m_DividerPrefab = null;
        [SerializeField] private DMButtonUI m_ButtonPrefab = null;
        [SerializeField] private DMTextUI m_TextPrefab = null;
        [SerializeField] private DMSliderUI m_SliderPrefab = null;
        [SerializeField] private DMSelectorUI m_SelectorPrefab = null;

        [Header("Element Transforms")]
        [SerializeField] private RectTransform m_ElementRoot = null;
        [SerializeField] private RectTransform m_ElementPool = null;

        [Header("Selection Arrow")]
        [SerializeField] private RectTransform m_SelectionArrow = null;
        [SerializeField] private bool m_AdjustSelectionOnHover = false;

        [Header("Settings")]
        [SerializeField] private int m_IndentSpacing = 16;
        [SerializeField] private int m_MaxElementsPerPage = 20;
        [SerializeField] private DMElementHeights m_HeightConfiguration = DMElementHeights.Default;

        #endregion // Inspector

        [NonSerialized] private bool m_Initialized;

        private RingBuffer<MenuStackItem> m_MenuStack = new RingBuffer<MenuStackItem>(4, RingBufferMode.Expand);
        private MenuStackItem m_CurrentMenu = MenuStackItem.Empty;
        [NonSerialized] private int m_CurrentPageCount = 1;
        private RingBuffer<OffsetLength32> m_PageDivisions = new RingBuffer<OffsetLength32>(4, RingBufferMode.Expand);
        [NonSerialized] private DMInteractableUI m_CurrentArrowItem;

        private RingBuffer<RectTransform> m_InactiveDividers = new RingBuffer<RectTransform>();
        private RingBuffer<DMButtonUI> m_InactiveButtons = new RingBuffer<DMButtonUI>();
        private RingBuffer<DMTextUI> m_InactiveTexts = new RingBuffer<DMTextUI>();
        private RingBuffer<DMSliderUI> m_InactiveSliders = new RingBuffer<DMSliderUI>();
        private RingBuffer<DMSelectorUI> m_InactiveSelectors = new RingBuffer<DMSelectorUI>();

        private RingBuffer<DMInteractableUI> m_InteractableOrder = new RingBuffer<DMInteractableUI>();

        private RingBuffer<RectTransform> m_ActiveDividers = new RingBuffer<RectTransform>();
        private RingBuffer<DMButtonUI> m_ActiveButtons = new RingBuffer<DMButtonUI>();
        private RingBuffer<DMTextUI> m_ActiveTexts = new RingBuffer<DMTextUI>();
        private RingBuffer<DMSliderUI> m_ActiveSliders = new RingBuffer<DMSliderUI>();
        private RingBuffer<DMSelectorUI> m_ActiveSelectors = new RingBuffer<DMSelectorUI>();
        [NonSerialized] private int m_SiblingIndexStart;

        private Action<DMButtonUI> m_CachedButtonOnClick;
        private Action<DMSliderUI, float> m_CachedSliderOnChange;
        private Action<DMSelectorUI, int> m_CachedSelectorOnChange;

        private PointerEventData m_ClickEvent;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (m_Initialized)
                return;

            m_SiblingIndexStart = m_ElementRoot.childCount;
            m_CachedButtonOnClick = m_CachedButtonOnClick ?? OnButtonClicked;
            m_CachedSliderOnChange = m_CachedSliderOnChange ?? OnSliderChanged;
            m_CachedSelectorOnChange = m_CachedSelectorOnChange ?? OnSelectorChanged;
            m_Header.SetBackCallback(OnBackClicked);
            m_Page.SetPageCallback(OnPageChanged);
            m_Initialized = true;

            m_ClickEvent = new PointerEventData(EventSystem.current);
            m_ClickEvent.button = PointerEventData.InputButton.Left;
        }

        #region Population

        private void PopulateMenu(DMInfo inMenu, int inPageIndex, bool inbForce)
        {
            EnsureInitialized();

            if (!inbForce && (m_CurrentMenu.Menu == inMenu && m_CurrentMenu.PageIndex == inPageIndex))
                return;

            m_CurrentMenu.Menu = inMenu;

            if (inMenu == null)
            {
                Clear();
                return;
            }

            int maxPages;
            GetPageSettings(inMenu, out maxPages);

            m_CurrentMenu.PageIndex = Mathf.Clamp(inPageIndex, 0, maxPages - 1);
            m_MenuStack[m_MenuStack.Count - 1] = m_CurrentMenu;
            m_CurrentPageCount = maxPages;

            m_Header.Init(inMenu.Header, inMenu.MinimumWidth, m_MenuStack.Count > 1);

            float minWidth = inMenu.MinimumWidth;

            int usedButtons = 0;
            int usedTexts = 0;
            int usedDividers = 0;
            int usedSliders = 0;
            int usedSelectors = 0;
            int siblingIndex = m_SiblingIndexStart;

            m_InteractableOrder.Clear();

            OffsetLength32 pageRange = m_PageDivisions[m_CurrentMenu.PageIndex];
            int elementOffset = pageRange.Offset;
            int elementCount = pageRange.Length;
            int elementIndex;
            DMElementType prevElementType = (DMElementType) 255;
            for (int i = 0; i < elementCount; i++)
            {
                elementIndex = i + elementOffset;

                DMElementInfo info = inMenu.Elements[elementIndex];
                minWidth = (float) Math.Max(info.MinimumWidth, minWidth);

                switch (info.Type)
                {
                    case DMElementType.Divider:
                    {
                        if (i > 0 && i < elementCount - 1 && prevElementType != DMElementType.Divider) // don't show unnecessary dividers
                        {
                            RectTransform divider;
                            if (usedDividers >= m_ActiveDividers.Count)
                            {
                                divider = AllocDivider();
                            }
                            else
                            {
                                divider = m_ActiveDividers[usedDividers];
                            }
                            divider.SetSiblingIndex(siblingIndex++);
                            ++usedDividers;
                        }
                        break;
                    }

                    case DMElementType.Button:
                    case DMElementType.Toggle:
                    case DMElementType.Submenu:
                    {
                        DMButtonUI button;
                        if (usedButtons >= m_ActiveButtons.Count)
                        {
                            button = AllocButton();
                        }
                        else
                        {
                            button = m_ActiveButtons[usedButtons];
                        }
                        button.Initialize(elementIndex, info, m_CachedButtonOnClick, m_IndentSpacing * info.Indent, m_InteractableOrder.Count, this);
                        button.transform.SetSiblingIndex(siblingIndex++);
                        usedButtons++;

                        m_InteractableOrder.PushBack(button.Interactable);
                        break;
                    }

                    case DMElementType.Text:
                    {
                        DMTextUI text;
                        if (usedTexts >= m_ActiveTexts.Count)
                        {
                            text = AllocText();
                        }
                        else
                        {
                            text = m_ActiveTexts[usedTexts];
                        }
                        text.Initialize(elementIndex, info, m_IndentSpacing * info.Indent);
                        text.transform.SetSiblingIndex(siblingIndex++);
                        usedTexts++;
                        break;
                    }

                    case DMElementType.Slider:
                    {
                        DMSliderUI slider;
                        if (usedSliders >= m_ActiveSliders.Count)
                        {
                            slider = AllocSlider();
                        }
                        else
                        {
                            slider = m_ActiveSliders[usedSliders];
                        }
                        slider.Initialize(elementIndex, info, m_CachedSliderOnChange, m_IndentSpacing * info.Indent, m_InteractableOrder.Count, this);
                        slider.transform.SetSiblingIndex(siblingIndex++);
                        usedSliders++;

                        m_InteractableOrder.PushBack(slider.Interactable);
                        break;
                    }

                    case DMElementType.Selector:
                    {
                        DMSelectorUI selector;
                        if (usedSelectors >= m_ActiveSelectors.Count)
                        {
                            selector = AllocSelector();
                        }
                        else
                        {
                            selector = m_ActiveSelectors[usedSelectors];
                        }
                        selector.Initialize(elementIndex, info, m_CachedSelectorOnChange, m_IndentSpacing * info.Indent, m_InteractableOrder.Count, this);
                        selector.transform.SetSiblingIndex(siblingIndex++);
                        usedSelectors++;

                        m_InteractableOrder.PushBack(selector.Interactable);
                        break;
                    }
                }

                prevElementType = info.Type;
            }

            m_Header.UpdateMinimumWidth(minWidth);

            int buttonsToRemove = m_ActiveButtons.Count - usedButtons;
            while (buttonsToRemove > 0)
            {
                DMButtonUI button = m_ActiveButtons.PopBack();
                button.transform.SetParent(m_ElementPool, false);
                m_InactiveButtons.PushBack(button);
                buttonsToRemove--;
            }

            int textsToRemove = m_ActiveTexts.Count - usedTexts;
            while (textsToRemove > 0)
            {
                DMTextUI text = m_ActiveTexts.PopBack();
                text.transform.SetParent(m_ElementPool, false);
                m_InactiveTexts.PushBack(text);
                textsToRemove--;
            }

            int dividersToRemove = m_ActiveDividers.Count - usedDividers;
            while (dividersToRemove > 0)
            {
                RectTransform divider = m_ActiveDividers.PopBack();
                divider.transform.SetParent(m_ElementPool, false);
                m_InactiveDividers.PushBack(divider);
                dividersToRemove--;
            }

            int slidersToRemove = m_ActiveSliders.Count - usedSliders;
            while (slidersToRemove > 0)
            {
                DMSliderUI slider = m_ActiveSliders.PopBack();
                slider.transform.SetParent(m_ElementPool, false);
                m_InactiveSliders.PushBack(slider);
                slidersToRemove--;
            }

            int selectorsToRemove = m_ActiveSelectors.Count - usedSelectors;
            while (selectorsToRemove > 0)
            {
                DMSelectorUI selector = m_ActiveSelectors.PopBack();
                selector.transform.SetParent(m_ElementPool, false);
                m_InactiveSelectors.PushBack(selector);
                selectorsToRemove--;
            }

            UpdateArrow(Math.Min(m_CurrentMenu.ArrowIndex, m_InteractableOrder.Count - 1), true);

            if (maxPages > 1)
            {
                m_Page.gameObject.SetActive(true);
                m_Page.transform.SetSiblingIndex(siblingIndex++);
                m_Page.UpdatePage(m_CurrentMenu.PageIndex, maxPages);
            }
            else
            {
                m_Page.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Clears state.
        /// </summary>
        public void Clear()
        {
            EnsureInitialized();

            ExitCurrent();

            m_CurrentMenu = MenuStackItem.Empty;
            m_MenuStack.Clear();
            m_PageDivisions.Clear();

            foreach (var button in m_ActiveButtons)
            {
                button.transform.SetParent(m_ElementPool, false);
                m_InactiveButtons.PushBack(button);
            }

            foreach (var text in m_ActiveTexts)
            {
                text.transform.SetParent(m_ElementPool, false);
                m_InactiveTexts.PushBack(text);
            }

            foreach (var divider in m_ActiveDividers)
            {
                divider.transform.SetParent(m_ElementPool, false);
                m_InactiveDividers.PushBack(divider);
            }

            foreach (var slider in m_ActiveSliders)
            {
                slider.transform.SetParent(m_ElementPool, false);
                m_InactiveSliders.PushBack(slider);
            }

            foreach (var selector in m_ActiveSelectors)
            {
                selector.transform.SetParent(m_ElementPool, false);
                m_InactiveSelectors.PushBack(selector);
            }

            m_ActiveButtons.Clear();
            m_ActiveTexts.Clear();
            m_ActiveDividers.Clear();
            m_ActiveSliders.Clear();
            m_ActiveSelectors.Clear();
            m_InteractableOrder.Clear();

            UpdateArrow(-1, true);

            m_Header.Init(new DMHeaderInfo() { Label = string.Empty }, 0, false);
        }

        private RectTransform AllocDivider()
        {
            RectTransform divider;
            if (!m_InactiveDividers.TryPopBack(out divider))
            {
                divider = Instantiate(m_DividerPrefab, m_ElementRoot);
            }
            else
            {
                divider.transform.SetParent(m_ElementRoot, false);
            }
            m_ActiveDividers.PushBack(divider);
            return divider;
        }

        private DMButtonUI AllocButton()
        {
            DMButtonUI button;
            if (!m_InactiveButtons.TryPopBack(out button))
            {
                button = Instantiate(m_ButtonPrefab, m_ElementRoot);
            }
            else
            {
                button.transform.SetParent(m_ElementRoot, false);
            }
            m_ActiveButtons.PushBack(button);
            return button;
        }

        private DMTextUI AllocText()
        {
            DMTextUI text;
            if (!m_InactiveTexts.TryPopBack(out text))
            {
                text = Instantiate(m_TextPrefab, m_ElementRoot);
            }
            else
            {
                text.transform.SetParent(m_ElementRoot, false);
            }
            m_ActiveTexts.PushBack(text);
            return text;
        }

        private DMSliderUI AllocSlider()
        {
            DMSliderUI slider;
            if (!m_InactiveSliders.TryPopBack(out slider))
            {
                slider = Instantiate(m_SliderPrefab, m_ElementRoot);
            }
            else
            {
                slider.transform.SetParent(m_ElementRoot, false);
            }
            m_ActiveSliders.PushBack(slider);
            return slider;
        }

        private DMSelectorUI AllocSelector()
        {
            DMSelectorUI selector;
            if (!m_InactiveSelectors.TryPopBack(out selector))
            {
                selector = Instantiate(m_SelectorPrefab, m_ElementRoot);
            }
            else
            {
                selector.transform.SetParent(m_ElementRoot, false);
            }
            m_ActiveSelectors.PushBack(selector);
            return selector;
        }

        private void GetPageSettings(DMInfo inMenu, out int outMaxPages)
        {
            m_PageDivisions.Clear();

            if (m_MaxElementsPerPage <= 0 || inMenu.Elements.Count <= 0)
            {
                outMaxPages = 1;
                m_PageDivisions.PushBack(new OffsetLength32(0, inMenu.Elements.Count));
            }
            else
            {
                int pageStartIndex = 0;
                int pageLength = 0;
                int pageHeight = 0;

                for (int i = 0; i < inMenu.Elements.Count; i++)
                {
                    DMElementType elementType = inMenu.Elements[i].Type;
                    if (elementType == DMElementType.PageBreak)
                    {
                        if (pageLength > 0)
                        {
                            m_PageDivisions.PushBack(new OffsetLength32(pageStartIndex, pageLength));
                            pageStartIndex = i + 1;
                            pageLength = 0;
                            pageHeight = 0;
                        }
                    }
                    else
                    {
                        int elementHeight = GetHeightForElementType(m_HeightConfiguration, elementType);
                        Assert.True(m_MaxElementsPerPage >= elementHeight);
                        if (pageHeight + elementHeight > m_MaxElementsPerPage)
                        {
                            m_PageDivisions.PushBack(new OffsetLength32(pageStartIndex, pageLength));
                            pageStartIndex = i;
                            pageLength = 0;
                            pageHeight = 0;
                        }

                        pageHeight += elementHeight;
                        pageLength++;
                    }
                }

                if (pageLength > 0)
                {
                    m_PageDivisions.PushBack(new OffsetLength32(pageStartIndex, pageLength));
                }

                outMaxPages = m_PageDivisions.Count;
            }
        }

        static private int GetHeightForElementType(in DMElementHeights inHeights, DMElementType inType)
        {
            switch (inType)
            {
                case DMElementType.Button:
                case DMElementType.Toggle:
                case DMElementType.Submenu:
                    return inHeights.Button;
                case DMElementType.Text:
                    return inHeights.Text;
                case DMElementType.Slider:
                    return inHeights.Slider;
                case DMElementType.Selector:
                    return inHeights.Selector;
                case DMElementType.TextInput:
                    return inHeights.TextInput;
                default:
                    return 0;
            }
        }

        #endregion // Population

        #region Menu Management

        /// <summary>
        /// Clears the menu stack and goes to a new menu.
        /// </summary>
        public void GotoMenu(DMInfo inMenu)
        {
            ExitCurrent();
            m_MenuStack.Clear();
            PushMenu(inMenu);
        }

        /// <summary>
        /// Pushes a new menu onto the stack.
        /// </summary>
        public void PushMenu(DMInfo inMenu)
        {
            int existingIndex = IndexOfMenu(inMenu);
            if (existingIndex >= 0)
            {
                int removeCount = m_MenuStack.Count - 1 - existingIndex;
                ExitCurrent();

                if (removeCount > 0)
                {
                    m_MenuStack.RemoveFromBack(removeCount);
                }
            }
            else
            {
                m_MenuStack.PushBack(new MenuStackItem(inMenu, 0, -1));
            }

            PopulateMenu(inMenu, 0, false);
            EnterCurrent();
        }

        /// <summary>
        /// Returns to the previously accessed menu.
        /// </summary>
        public void PopMenu()
        {
            if (m_MenuStack.Count > 1)
            {
                ExitCurrent();

                m_MenuStack.PopBack();
                MenuStackItem next;
                m_MenuStack.TryPeekBack(out next);
                m_CurrentMenu.ArrowIndex = next.ArrowIndex;
                PopulateMenu(next.Menu, next.PageIndex, false);

                EnterCurrent();
            }
            else
            {
                throw new InvalidOperationException("Cannot pop from an root menu stack");
            }
        }

        /// <summary>
        /// Attempts to return to the previous menu
        /// </summary>
        public bool TryPopMenu()
        {
            if (m_MenuStack.Count > 1)
            {
                ExitCurrent();

                m_MenuStack.PopBack();
                MenuStackItem next;
                m_MenuStack.TryPeekBack(out next);
                m_CurrentMenu.ArrowIndex = next.ArrowIndex;
                PopulateMenu(next.Menu, next.PageIndex, false);

                EnterCurrent();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to move to the next page.
        /// </summary>
        public bool TryNextPage()
        {
            if (m_MenuStack.Count <= 0 || m_CurrentPageCount <= 1 || m_CurrentMenu.PageIndex >= m_CurrentPageCount - 1)
                return false;

            PopulateMenu(m_CurrentMenu.Menu, m_CurrentMenu.PageIndex + 1, false);
            return true;
        }

        /// <summary>
        /// Attempts to move to the previous page.
        /// </summary>
        public bool TryPreviousPage()
        {
            if (m_MenuStack.Count <= 0 || m_CurrentPageCount <= 1 || m_CurrentMenu.PageIndex <= 0)
                return false;

            PopulateMenu(m_CurrentMenu.Menu, m_CurrentMenu.PageIndex - 1, false);
            return true;
        }

        private int IndexOfMenu(DMInfo inMenu)
        {
            for (int i = 0, len = m_MenuStack.Count; i < len; i++)
            {
                if (m_MenuStack[i].Menu == inMenu)
                    return i;
            }

            return -1;
        }

        private void ExitCurrent()
        {
            if (isActiveAndEnabled)
            {
                if (m_CurrentMenu.Menu != null)
                {
                    m_CurrentMenu.Menu.OnExit.Invoke(m_CurrentMenu.Menu);
                }
            }
        }

        private void EnterCurrent()
        {
            if (isActiveAndEnabled)
            {
                if (m_CurrentMenu.Menu != null)
                {
                    m_CurrentMenu.Menu.OnEnter.Invoke(m_CurrentMenu.Menu);
                }
            }
        }

        #endregion // Menu Management

        #region Selection Management

        private void UpdateArrow(int inArrowIdx, bool inbForce)
        {
            if (!inbForce && m_CurrentMenu.ArrowIndex == inArrowIdx)
            {
                return;
            }

            m_CurrentMenu.ArrowIndex = inArrowIdx;
            if (m_MenuStack.Count > 0)
            {
                m_MenuStack[m_MenuStack.Count - 1] = m_CurrentMenu;
            }
            if (inArrowIdx < 0)
            {
                UpdateArrowObject(null, inbForce);
            }
            else
            {
                UpdateArrowObject(m_InteractableOrder[inArrowIdx], inbForce);
            }
        }

        private void UpdateArrowObject(DMInteractableUI inInteractable, bool inbForce)
        {
            if (!inbForce && inInteractable == m_CurrentArrowItem)
            {
                return;
            }

            if (inInteractable == null && m_CurrentArrowItem)
            {
                var evtSys = EventSystem.current;
                if (evtSys != null && evtSys.currentSelectedGameObject == m_CurrentArrowItem.Selectable.gameObject)
                {
                    evtSys.SetSelectedGameObject(null);
                }
            }

            m_CurrentArrowItem = inInteractable;

            if (inInteractable)
            {
                m_SelectionArrow.gameObject.SetActive(true);
                inInteractable.Selectable.Select();

                RectTransform arrowRect = (RectTransform) m_SelectionArrow.transform;
                RectTransform interactableRect = (RectTransform) inInteractable.transform;

                arrowRect.SetParent(interactableRect, true);

                Vector2 anchor = arrowRect.anchoredPosition;
                anchor.y = 0;
                arrowRect.anchoredPosition = anchor;
            }
            else
            {
                m_SelectionArrow.SetParent(m_ElementPool, true);
                m_SelectionArrow.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Scrolls the selection arrow in the given direction.
        /// </summary>
        public void ScrollArrow(int inShift)
        {
            if (m_InteractableOrder.Count > 0)
            {
                UpdateArrow((m_CurrentMenu.ArrowIndex + inShift + m_InteractableOrder.Count) % m_InteractableOrder.Count, false);
            }
        }

        public bool SubmitCommand(NavigationCommand inCommand)
        {
            switch (inCommand)
            {
                case NavigationCommand.NextPage:
                    return TryNextPage();

                case NavigationCommand.PrevPage:
                    return TryPreviousPage();

                case NavigationCommand.Back:
                    return TryPopMenu();

                case NavigationCommand.MoveArrowDown:
                    ScrollArrow(1);
                    return true;

                case NavigationCommand.MoveArrowUp:
                    ScrollArrow(-1);
                    return true;

                case NavigationCommand.SelectArrow:
                {
                    if (m_CurrentArrowItem != null)
                    {
                        Selectable s = m_CurrentArrowItem.Selectable;
                        if (!s.IsInteractable())
                            return false;

                        Button button;
                        if (button = s as Button)
                        {
                            button.onClick.Invoke();
                            return true;
                        }
                    }
                    return false;
                }

                case NavigationCommand.ClearArrow:
                {
                    bool cancelledCurrent = m_CurrentMenu.ArrowIndex >= 0;
                    UpdateArrow(-1, false);
                    for(int i = 0; i < m_MenuStack.Count; i++)
                    {
                        m_MenuStack[i].ArrowIndex = -1;
                    }
                    return cancelledCurrent;
                }

                case NavigationCommand.IncreaseSlider:
                case NavigationCommand.DecreaseSlider:
                {
                    if (m_CurrentArrowItem != null)
                    {
                        Selectable s = m_CurrentArrowItem.Selectable;
                        if (!s.IsInteractable())
                            return false;

                        DMSliderUI sliderUI = m_CurrentArrowItem.GetComponent<DMSliderUI>();
                        if (sliderUI != null)
                        {
                            sliderUI.AdjustValueInTicks(inCommand == NavigationCommand.IncreaseSlider ? 1 : -1);
                            return true;
                        }

                        DMSelectorUI selectorUI = m_CurrentArrowItem.GetComponent<DMSelectorUI>();
                        if (selectorUI != null)
                        {
                            selectorUI.AdjustValueInTicks(inCommand == NavigationCommand.IncreaseSlider ? 1 : -1);
                            return true;
                        }
                    }
                    return false;
                }

                default:
                    return false;
            }
        }

        #endregion // Selection Management

        #region Update

        /// <summary>
        /// Refreshes the current menu.
        /// </summary>
        public void RefreshMenu()
        {
            PopulateMenu(m_CurrentMenu.Menu, m_CurrentMenu.PageIndex, true);
        }

        /// <summary>
        /// Updates all elements displayed in the current menu.
        /// </summary>
        public void UpdateElements()
        {
            StringBuilder sb = DMInfo.SharedTextBuilder;

            foreach (var button in m_ActiveButtons)
            {
                DMElementInfo info = m_CurrentMenu.Menu.Elements[button.ElementIndex];
                button.Interactable.UpdateInteractive(DMInfo.EvaluateOptionalPredicate(info.Predicate));

                switch (info.Type)
                {
                    case DMElementType.Toggle:
                    {
                        button.UpdateToggleState(info.Toggle.Getter());
                        break;
                    }
                }
            }

            foreach (var text in m_ActiveTexts)
            {
                DMTextInfo textInfo = m_CurrentMenu.Menu.Elements[text.ElementIndex].Text;

                if (textInfo.Getter != null)
                {
                    text.UpdateValue(textInfo.Getter());
                }
                else if (textInfo.GetterSB != null)
                {
                    sb.Clear();
                    textInfo.GetterSB(sb);
                    text.UpdateValue(sb);
                }
            }

            sb.Clear();

            foreach (var slider in m_ActiveSliders)
            {
                DMElementInfo info = m_CurrentMenu.Menu.Elements[slider.ElementIndex];
                slider.Interactable.UpdateInteractive(DMInfo.EvaluateOptionalPredicate(info.Predicate));
                slider.UpdateValue(info.Slider.Getter(), info.Slider);
            }

            foreach (var selector in m_ActiveSelectors)
            {
                DMElementInfo info = m_CurrentMenu.Menu.Elements[selector.ElementIndex];
                selector.Interactable.UpdateInteractive(DMInfo.EvaluateOptionalPredicate(info.Predicate));
                selector.UpdateValue(info.Selector.Getter(), info.Selector);
            }
        }

        #endregion // Update

        #region Handlers

        private void OnButtonClicked(DMButtonUI inButton)
        {
            UpdateArrow(inButton.Interactable.InteractableIndex, false);

            int index = inButton.ElementIndex;
            DMElementInfo info = m_CurrentMenu.Menu.Elements[index];
            switch (info.Type)
            {
                case DMElementType.Button:
                {
                    info.Button.Callback();
                    UpdateElements();
                    break;
                }

                case DMElementType.Toggle:
                {
                    bool bNewValue = !inButton.ToggleState();
                    info.Toggle.Setter(bNewValue);
                    inButton.UpdateToggleState(bNewValue);
                    UpdateElements();
                    break;
                }

                case DMElementType.Submenu:
                {
                    PushMenu(info.Submenu.Submenu);
                    break;
                }
            }
        }

        private void OnSliderChanged(DMSliderUI inSlider, float inPercentage)
        {
            UpdateArrow(inSlider.Interactable.InteractableIndex, false);

            int index = inSlider.ElementIndex;
            DMElementInfo info = m_CurrentMenu.Menu.Elements[index];

            float realValue = DMSliderRange.GetValue(info.Slider.Range, inPercentage);
            info.Slider.Setter(realValue);

            if (inSlider.UpdateValue(realValue, info.Slider))
            {
                UpdateElements();
            }
        }

        private void OnSelectorChanged(DMSelectorUI inSelector, int inIndex)
        {
            UpdateArrow(inSelector.Interactable.InteractableIndex, false);

            int index = inSelector.ElementIndex;
            DMElementInfo info = m_CurrentMenu.Menu.Elements[index];

            DMSelectorInfo selectorInfo = info.Selector;

            int realValue = DMSelectorInfo.GetValue(selectorInfo, inIndex);
            info.Selector.Setter(realValue);

            if (inSelector.UpdateValue(realValue, selectorInfo))
            {
                UpdateElements();
            }
        }

        private void OnPageChanged(int inPageIndex)
        {
            PopulateMenu(m_CurrentMenu.Menu, inPageIndex, false);
        }

        private void OnBackClicked()
        {
            PopMenu();
        }

        internal void OnInteractableHover(DMInteractableUI inInteractable)
        {
            if (!m_AdjustSelectionOnHover)
                return;

            UpdateArrow(inInteractable.InteractableIndex, false);
        }

        #endregion // Handlers
    }
}