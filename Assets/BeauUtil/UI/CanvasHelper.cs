/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    CanvasHelper.cs
 * Purpose: Helper methods for canvas objects.
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
            switch(inCanvas.renderMode)
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
            for(int i = 0, count = inTransform.childCount; i < count; i++)
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
            for(int i = 0, count = inTransform.childCount; i < count; i++)
            {
                RectTransform r = inTransform.GetChild(i) as RectTransform;
                if (r != null)
                    RecursiveMarkForRebuild(r);
            }

            ILayoutController controller = inTransform.GetComponent<ILayoutController>();
            if (controller != null)
                LayoutRebuilder.MarkLayoutForRebuild(inTransform);
        }

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
            while(t != null)
            {
                t.GetComponents(components);
                for(int i = 0, len = components.Count; i < len; ++i)
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
    }
}