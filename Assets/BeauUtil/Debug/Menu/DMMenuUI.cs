/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMTextUI.cs
 * Purpose: Debug menu display code.
 */

using System;
using UnityEngine;

namespace BeauUtil.Debugger
{
    public class DMMenuUI : MonoBehaviour
    {
        private struct MenuStackItem
        {
            public DMInfo Menu;
            public int PageIndex;

            public MenuStackItem(DMInfo inMenu, int inPageIndex)
            {
                Menu = inMenu;
                PageIndex = inPageIndex;
            }
        }

        #region Inspector

        [SerializeField] private DMHeaderUI m_Header = null;
        [SerializeField] private DMPageUI m_Page = null;

        [Header("Prefabs")]
        [SerializeField] private RectTransform m_DividerPrefab = null;
        [SerializeField] private DMButtonUI m_ButtonPrefab = null;
        [SerializeField] private DMTextUI m_TextPrefab = null;

        [Header("Element Transforms")]
        [SerializeField] private RectTransform m_ElementRoot = null;
        [SerializeField] private RectTransform m_ElementPool = null;

        [Header("Settings")]
        [SerializeField] private int m_IndentSpacing = 16;
        [SerializeField] private int m_MaxElementsPerPage = 20;
        
        #endregion // Inspector

        [NonSerialized] private bool m_Initialized;

        private RingBuffer<MenuStackItem> m_MenuStack = new RingBuffer<MenuStackItem>(4, RingBufferMode.Expand);
        private MenuStackItem m_CurrentMenu;

        private RingBuffer<RectTransform> m_InactiveDividers = new RingBuffer<RectTransform>();
        private RingBuffer<DMButtonUI> m_InactiveButtons = new RingBuffer<DMButtonUI>();
        private RingBuffer<DMTextUI> m_InactiveTexts = new RingBuffer<DMTextUI>();

        private RingBuffer<RectTransform> m_ActiveDividers = new RingBuffer<RectTransform>();
        private RingBuffer<DMButtonUI> m_ActiveButtons = new RingBuffer<DMButtonUI>();
        private RingBuffer<DMTextUI> m_ActiveTexts = new RingBuffer<DMTextUI>();
        private int m_SiblingIndexStart;

        private Action<DMButtonUI> m_CachedButtonOnClick;

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
            m_Header.SetBackCallback(OnBackClicked);
            m_Page.SetPageCallback(OnPageChanged);
            m_Initialized = true;
        }

        #region Population

        private void PopulateMenu(DMInfo inMenu, int inPageIndex)
        {
            EnsureInitialized();

            if (m_CurrentMenu.Menu == inMenu && m_CurrentMenu.PageIndex == inPageIndex)
                return;

            m_CurrentMenu.Menu = inMenu;

            if (inMenu == null)
            {
                Clear();
                return;
            }

            int elementsPerPage, maxPages;
            GetPageSettings(inMenu, out maxPages, out elementsPerPage);

            m_CurrentMenu.PageIndex = Mathf.Clamp(inPageIndex, 0, maxPages - 1);
            m_MenuStack[m_MenuStack.Count - 1] = m_CurrentMenu;

            m_Header.Init(inMenu.Header, m_MenuStack.Count > 1);

            int usedButtons = 0;
            int usedTexts = 0;
            int usedDividers = 0;
            int siblingIndex = m_SiblingIndexStart;

            int elementOffset = m_CurrentMenu.PageIndex * elementsPerPage;
            int elementCount = Math.Min(elementsPerPage, inMenu.Elements.Count - elementOffset);
            int elementIndex;
            for(int i = 0; i < elementCount; i++)
            {
                elementIndex = i + elementOffset;

                DMElementInfo info = inMenu.Elements[elementIndex];
                switch(info.Type)
                {
                    case DMElementType.Divider:
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
                            button.Initialize(elementIndex, info, m_CachedButtonOnClick, m_IndentSpacing * info.Indent);
                            button.transform.SetSiblingIndex(siblingIndex++);
                            usedButtons++;
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
                }
            }

            int buttonsToRemove = m_ActiveButtons.Count - usedButtons;
            while(buttonsToRemove > 0)
            {
                DMButtonUI button = m_ActiveButtons.PopBack();
                button.transform.SetParent(m_ElementPool, false);
                m_InactiveButtons.PushBack(button);
                buttonsToRemove--;
            }

            int textsToRemove = m_ActiveTexts.Count - usedTexts;
            while(textsToRemove  > 0)
            {
                DMTextUI text = m_ActiveTexts.PopBack();
                text.transform.SetParent(m_ElementPool, false);
                m_InactiveTexts.PushBack(text);
                textsToRemove--;
            }

            int dividersToRemove = m_ActiveDividers.Count - usedDividers;
            while(dividersToRemove  > 0)
            {
                RectTransform divider = m_ActiveDividers.PopBack();
                divider.transform.SetParent(m_ElementPool, false);
                m_InactiveDividers.PushBack(divider);
                dividersToRemove--;
            }

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

            m_CurrentMenu = default(MenuStackItem);
            m_MenuStack.Clear();

            foreach(var button in m_ActiveButtons)
            {
                button.transform.SetParent(m_ElementPool, false);
                m_InactiveButtons.PushBack(button);
            }

            foreach(var text in m_ActiveTexts)
            {
                text.transform.SetParent(m_ElementPool, false);
                m_InactiveTexts.PushBack(text);
            }

            foreach(var divider in m_ActiveDividers)
            {
                divider.transform.SetParent(m_ElementPool, false);
                m_InactiveDividers.PushBack(divider);
            }

            m_ActiveButtons.Clear();
            m_ActiveTexts.Clear();
            m_ActiveDividers.Clear();

            m_Header.Init(new DMHeaderInfo() { Label = string.Empty }, false);
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

        private void GetPageSettings(DMInfo inMenu, out int outMaxPages, out int outElementsPerPage)
        {
            if (m_MaxElementsPerPage <= 0 || inMenu.Elements.Count <= 0)
            {
                outMaxPages = 1;
                outElementsPerPage = inMenu.Elements.Count;
            }
            else
            {
                outMaxPages = (int) Math.Ceiling((float) inMenu.Elements.Count / m_MaxElementsPerPage);
                outElementsPerPage = m_MaxElementsPerPage;
            }
        }

        #endregion // Population

        #region Menu Management

        /// <summary>
        /// Clears the menu stack and goes to a new menu.
        /// </summary>
        public void GotoMenu(DMInfo inMenu)
        {
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
                if (removeCount > 0)
                    m_MenuStack.RemoveFromBack(removeCount);
            }
            else
            {
                m_MenuStack.PushBack(new MenuStackItem(inMenu, 0));
            }
            
            PopulateMenu(inMenu, 0);
        }

        /// <summary>
        /// Returns to the previously accessed menu.
        /// </summary>
        public void PopMenu()
        {
            if (m_MenuStack.Count > 1)
            {
                m_MenuStack.PopBack();
                MenuStackItem prevMenu;
                m_MenuStack.TryPeekBack(out prevMenu);
                PopulateMenu(prevMenu.Menu, prevMenu.PageIndex);
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
                m_MenuStack.PopBack();
                MenuStackItem prevMenu;
                m_MenuStack.TryPeekBack(out prevMenu);
                PopulateMenu(prevMenu.Menu, prevMenu.PageIndex);
                return true;
            }

            return false;
        }

        private int IndexOfMenu(DMInfo inMenu)
        {
            for(int i = 0, len = m_MenuStack.Count; i < len; i++)
            {
                if (m_MenuStack[i].Menu == inMenu)
                    return i;
            }

            return -1;
        }

        #endregion // Menu Management

        #region Update

        /// <summary>
        /// Updates all element's 
        /// </summary>
        public void UpdateElements()
        {
            foreach(var button in m_ActiveButtons)
            {
                DMElementInfo info = m_CurrentMenu.Menu.Elements[button.ElementIndex];
                button.UpdateInteractive(DMInfo.EvaluateOptionalPredicate(info.Predicate));
                switch(info.Type)
                {
                    case DMElementType.Toggle:
                        {
                            button.UpdateToggleState(info.Toggle.Getter());
                            break;
                        }
                }
            }

            foreach(var text in m_ActiveTexts)
            {
                DMTextInfo textInfo = m_CurrentMenu.Menu.Elements[text.ElementIndex].Text;
                text.UpdateValue(textInfo.Getter());
            }
        }

        #endregion // Update

        #region Handlers

        private void OnButtonClicked(DMButtonUI inButton)
        {
            int index = inButton.ElementIndex;
            DMElementInfo info = m_CurrentMenu.Menu.Elements[index];
            switch(info.Type)
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

        private void OnPageChanged(int inPageIndex)
        {
            PopulateMenu(m_CurrentMenu.Menu, inPageIndex);
        }

        private void OnBackClicked()
        {
            PopMenu();
        }

        #endregion // Handlers
    }
}