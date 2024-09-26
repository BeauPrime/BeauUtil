/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    CanvasHelper.cs
 * Purpose: Helper methods for canvas objects.
 */

#if !USING_TINYIL || (!UNITY_EDITOR && !ENABLE_IL2CPP)
#define RESTRICT_INTERNAL_CALLS
#endif // !USING_TINYIL || (!UNITY_EDITOR && !ENABLE_IL2CPP)

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Reflection;

#if ENABLE_TEXTMESHPRO
using TMPro;
#endif // ENABLE_TEXTMESHPRO

namespace BeauUtil
{
    /// <summary>
    /// Canvas object helpers.
    /// </summary>
    static public class CanvasHelper
    {
        /// <summary>
        /// Attempts to get the default camera used to render this canvas.
        /// </summary>
        static public bool TryGetCamera(this Canvas inCanvas, out Camera outCamera)
        {
            switch (inCanvas.renderMode)
            {
                case RenderMode.WorldSpace:
                {
                    outCamera = inCanvas.worldCamera;
                    if (!outCamera)
                    {
                        return inCanvas.transform.TryGetCameraFromLayer(out outCamera);
                    }

                    return true;
                }

                case RenderMode.ScreenSpaceOverlay:
                {
                    outCamera = null;
                    return true;
                }

                case RenderMode.ScreenSpaceCamera:
                {
                    outCamera = inCanvas.worldCamera;
                    if (!outCamera)
                    {
                        outCamera = null;
                    }
                    return true;
                }

                default:
                    throw new InvalidOperationException("Camera mode " + inCanvas.renderMode + " is not recognized");
            }
        }

        /// <summary>
        /// Returns the first active toggle.
        /// </summary>
        static public Toggle ActiveToggle(this ToggleGroup inGroup)
        {
            IEnumerable<Toggle> all = inGroup.ActiveToggles();
            var iter = all.GetEnumerator();
            if (iter.MoveNext())
                return iter.Current;
            return null;
        }

        #region Layout

        /// <summary>
        /// Forces this layout group to rebuild itself.
        /// </summary>
        static public void ForceRebuild(this LayoutGroup inLayoutGroup, bool inbRecursive = true)
        {
            if (inbRecursive)
            {
                RecursiveLayoutRebuild((RectTransform) inLayoutGroup.transform);
            }
            else
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) inLayoutGroup.transform);
            }
        }

        /// <summary>
        /// Forces this layout group to rebuild itself.
        /// </summary>
        static public void ForceRebuild<T>(this T inLayoutGroup, bool inbRecursive = true) where T : Component, ILayoutController
        {
            if (inbRecursive)
            {
                RecursiveLayoutRebuild((RectTransform) inLayoutGroup.transform);
            }
            else
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) inLayoutGroup.transform);
            }
        }

        /// <summary>
        /// Recursivelye rebuilds layout
        /// </summary>
        static private void RecursiveLayoutRebuild(RectTransform inTransform)
        {
            for (int i = 0, count = inTransform.childCount; i < count; i++)
            {
                RectTransform r = inTransform.GetChild(i) as RectTransform;
                if (r != null)
                    RecursiveLayoutRebuild(r);
            }

            ILayoutController controller = inTransform.GetComponent<ILayoutController>();
            if (controller != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(inTransform);
        }

        /// <summary>
        /// Marks this layout group to be rebuilt.
        /// </summary>
        static public void MarkForRebuild(this LayoutGroup inLayoutGroup, bool inbRecursive = true)
        {
            if (inbRecursive)
            {
                RecursiveMarkForRebuild((RectTransform) inLayoutGroup.transform);
            }
            else
            {
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform) inLayoutGroup.transform);
            }
        }

        /// <summary>
        /// Marks this layout group to be rebuilt.
        /// </summary>
        static public void MarkForRebuild<T>(this T inLayoutGroup, bool inbRecursive = true) where T : Component, ILayoutController
        {
            if (inbRecursive)
            {
                RecursiveMarkForRebuild((RectTransform) inLayoutGroup.transform);
            }
            else
            {
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform) inLayoutGroup.transform);
            }
        }

        /// <summary>
        /// Recursivelye rebuilds layout
        /// </summary>
        static private void RecursiveMarkForRebuild(RectTransform inTransform)
        {
            for (int i = 0, count = inTransform.childCount; i < count; i++)
            {
                RectTransform r = inTransform.GetChild(i) as RectTransform;
                if (r != null)
                    RecursiveMarkForRebuild(r);
            }

            ILayoutController controller = inTransform.GetComponent<ILayoutController>();
            if (controller != null)
                LayoutRebuilder.MarkLayoutForRebuild(inTransform);
        }

        #endregion // Layout

        /// <summary>
        /// Returns if objects at the given RectTransform are interactable.
        /// This will check both for Raycasts and for Interactable on CanvasGroups.
        /// </summary>
        static public bool IsPointerInteractable(this RectTransform inRectTransform)
        {
            if (!inRectTransform.gameObject.activeInHierarchy)
                return false;

            Graphic graphic = inRectTransform.GetComponent<Graphic>();
            if (!ReferenceEquals(graphic, null) && !graphic.raycastTarget)
                return false;

            List<Component> components = s_CachedComponentList ?? (s_CachedComponentList = new List<Component>(8));
            Component c;

            Transform t = inRectTransform;
            bool bIgnoreParentGroups = false;
            bool bRaycastValid = true;
            bool bContinueTraversal = true;
            while (t != null)
            {
                t.GetComponents(components);
                for (int i = 0, len = components.Count; i < len; ++i)
                {
                    c = components[i];

                    Canvas canvas = c as Canvas;
                    if (!ReferenceEquals(canvas, null) && canvas.overrideSorting)
                    {
                        bRaycastValid = canvas.enabled;
                        bContinueTraversal = false;
                        continue;
                    }

                    GraphicRaycaster raycaster = c as GraphicRaycaster;
                    if (!ReferenceEquals(raycaster, null) && !raycaster.enabled)
                    {
                        bContinueTraversal = false;
                        bRaycastValid = false;
                        break;
                    }

                    CanvasGroup group = c as CanvasGroup;
                    if (!ReferenceEquals(group, null) && !bIgnoreParentGroups)
                    {
                        bIgnoreParentGroups = group.ignoreParentGroups;
                        bRaycastValid = group.interactable && group.blocksRaycasts;
                        if (!bRaycastValid)
                        {
                            bContinueTraversal = false;
                            break;
                        }
                    }

                    Selectable selectable = c as Selectable;
                    if (!ReferenceEquals(selectable, null) && !selectable.interactable)
                    {
                        bRaycastValid = false;
                        bContinueTraversal = false;
                        break;
                    }
                }

                t = bContinueTraversal ? t.parent : null;
            }

            components.Clear();
            return bRaycastValid;
        }

        [ThreadStatic]
        static private List<Component> s_CachedComponentList;

        #region Text

        private const int DefaultTextBufferSize = 256;

        static private int s_CurrentCharBufferSize = DefaultTextBufferSize;
        static private char[] s_CurrentCharBuffer = null;

        /// <summary>
        /// Size of the internal char buffer.
        /// Used when displaying text from an unsafe buffer.
        /// </summary>
        static public int TextBufferSize
        {
            get { return s_CurrentCharBufferSize; }
            set
            {
                if (value == s_CurrentCharBufferSize)
                    return;

                if (value != 0 && (value < 16 || value > ushort.MaxValue + 1))
                    throw new ArgumentOutOfRangeException("value", "Buffer size must be between 16 and 655356");

                if (value > 0 && !Mathf.IsPowerOfTwo(value))
                    throw new ArgumentException("Buffer size must be a power of 2", "value");

                s_CurrentCharBufferSize = value;
                Array.Resize(ref s_CurrentCharBuffer, value);
            }
        }

        #region TextMeshPro

#if ENABLE_TEXTMESHPRO

        /// <summary>
        /// Sets text on the given TextMeshPro element from an unsafe char buffer.
        /// If the text length exceeds the current <c>CanvasHelper.BufferSize</c>, then a string will be allocated.
        /// Otherwise this will not allocate any extra string memory.
        /// </summary>
        /// <remarks>
        /// Note:   In the editor, TextMeshPro will automatically allocate a string internally,
        ///         so it can be displayed in the inspector. This does not occur in builds.
        /// </remarks>
        static public unsafe void SetText(this TMP_Text inTextMeshPro, char* inCharBuffer, int inCharBufferLength)
        {
            if (inCharBuffer == null || inCharBufferLength <= 0)
            {
                inTextMeshPro.SetText(string.Empty);
                return;
            }

            if (inCharBufferLength > s_CurrentCharBufferSize)
            {
                Debug.LogWarningFormat("[CanvasHelper] Input text of length {0} exceeded buffer size {1} - consider adjusting buffer size", inCharBufferLength.ToString(), s_CurrentCharBufferSize.ToString());
                inTextMeshPro.SetText(new string(inCharBuffer, 0, inCharBufferLength));
                return;
            }

            if (s_CurrentCharBuffer == null)
            {
                s_CurrentCharBuffer = new char[s_CurrentCharBufferSize];
            }

            Unsafe.CopyArray(inCharBuffer, inCharBufferLength, s_CurrentCharBuffer);
            inTextMeshPro.SetText(s_CurrentCharBuffer, 0, inCharBufferLength);
        }

        /// <summary>
        /// Sets text on the given TextMeshPro element from a StringSlice.
        /// If the slice length exceeds the current <c>CanvasHelper.BufferSize</c>, then a string will be allocated.
        /// Otherwise this will not allocate any extra string memory.
        /// </summary>
        /// <remarks>
        /// Note:   In the editor, TextMeshPro will automatically allocate a string internally,
        ///         so it can be displayed in the inspector. This does not occur in builds.
        /// </remarks>
        static public void SetText(this TMP_Text inTextMeshPro, StringSlice inSlice)
        {
            if (inSlice.IsEmpty)
            {
                inTextMeshPro.SetText(string.Empty);
                return;
            }

            if (inSlice.Length > s_CurrentCharBufferSize)
            {
                Debug.LogWarningFormat("[CanvasHelper] Input text of length {0} exceeded buffer size {1} - consider adjusting buffer size", inSlice.Length.ToString(), s_CurrentCharBufferSize.ToString());
                inTextMeshPro.SetText(inSlice.ToString());
                return;
            }

            if (s_CurrentCharBuffer == null)
            {
                s_CurrentCharBuffer = new char[s_CurrentCharBufferSize];
            }

            string str;
            int offset, length;
            inSlice.Unpack(out str, out offset, out length);

            if (length == str.Length)
            {
                inTextMeshPro.SetText(str);
            }
            else
            {
                str.CopyTo(offset, s_CurrentCharBuffer, 0, length);
                inTextMeshPro.SetText(s_CurrentCharBuffer, 0, length);
            }
        }

        /// <summary>
        /// Sets text on the given TextMeshPro element from a StringBuilderSlice.
        /// If the slice length exceeds the current <c>CanvasHelper.BufferSize</c>, then a string will be allocated.
        /// Otherwise this will not allocate any extra string memory.
        /// </summary>
        /// <remarks>
        /// Note:   In the editor, TextMeshPro will automatically allocate a string internally,
        ///         so it can be displayed in the inspector. This does not occur in builds.
        /// </remarks>
        static public void SetText(this TMP_Text inTextMeshPro, StringBuilderSlice inSlice)
        {
            if (inSlice.IsEmpty)
            {
                inTextMeshPro.SetText(string.Empty);
                return;
            }

            StringBuilder str;
            int offset, length;
            inSlice.Unpack(out str, out offset, out length);

            if (length == str.Length)
            {
                inTextMeshPro.SetText(str);
            }
            else if (length > s_CurrentCharBufferSize)
            {
                Debug.LogWarningFormat("[CanvasHelper] Input text of length {0} exceeded buffer size {1} - consider adjusting buffer size", length.ToString(), s_CurrentCharBufferSize.ToString());
                inTextMeshPro.SetText(inSlice.ToString());
                return;
            }
            else
            {
                if (s_CurrentCharBuffer == null)
                {
                    s_CurrentCharBuffer = new char[s_CurrentCharBufferSize];
                }

                str.CopyTo(offset, s_CurrentCharBuffer, 0, length);
                inTextMeshPro.SetText(s_CurrentCharBuffer, 0, length);
            }
        }

#endif // ENABLE_TEXTMESHPRO

        #endregion // TextMeshPro

        #endregion // Text

        #region Selectable

#if RESTRICT_INTERNAL_CALLS
        static private MethodInfo s_Selectable_get_currentSelectionState;
# endif // RESTRICT_INTERNAL_CALLS

        static CanvasHelper()
        {
#if RESTRICT_INTERNAL_CALLS
            s_Selectable_get_currentSelectionState = typeof(Selectable).GetProperty("currentSelectionState", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetMethod;
#endif // RESTRICT_INTERNAL_CALLS
        }

        /// <summary>
        /// Returns the current selection state of the given selectable.
        /// </summary>
#if !RESTRICT_INTERNAL_CALLS
        [IntrinsicIL("ldarg.0; call [arg inSelectable]::get_currentSelectionState(); ret;")]
#endif // !RESTRICT_INTERNAL_CALLS
        static public SelectionState GetSelectionState(this Selectable inSelectable)
        {
#if RESTRICT_INTERNAL_CALLS
            if (!inSelectable.IsInteractable())
            {
                return SelectionState.Disabled;
            }
            if (s_Selectable_get_currentSelectionState == null)
            {
                return SelectionState.Normal;
            }

            return (SelectionState) Convert.ToInt32(s_Selectable_get_currentSelectionState.Invoke(inSelectable, null));
#else
            throw new NotImplementedException();
#endif // RESTRICT_INTERNAL_CALLS
        }

        #endregion // Selectable
    }

    /// <summary>
    /// Exposed selectable selection state.
    /// </summary>
    public enum SelectionState
    {
        Unset = -1,
        Normal = 0,
        Highlighted,
        Pressed,
        Selected,
        Disabled
    }
}