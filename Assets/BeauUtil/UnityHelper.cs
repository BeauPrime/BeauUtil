/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    UnityHelper.cs
 * Purpose: Unity-specific helper functions.
*/

namespace BeauUtil
{
    /// <summary>
    /// Contains Unity-specific utility functions.
    /// </summary>
    static public class UnityHelper
    {
        /// <summary>
        /// Safely disposes of a Unity object and sets the reference to null.
        /// </summary>
        static public void SafeDestroy<T>(ref T ioObject) where T : UnityEngine.Object
        {
            // This is to avoid calling Unity's overridden equality operator
            if (object.ReferenceEquals(ioObject, null))
                return;

            // This is to see if the object hasn't been destroyed yet
            if (ioObject)
            {
                UnityEngine.Object.Destroy(ioObject);
            }

            ioObject = null;
        }

        /// <summary>
        /// Safely disposes of the GameObject and sets
        /// the reference to null.
        /// </summary>
        static public void SafeDestroyGO(ref UnityEngine.GameObject ioGameObject)
        {
            // This is to avoid calling Unity's overridden equality operator
            if (object.ReferenceEquals(ioGameObject, null))
                return;

            // This is to see if the object hasn't been destroyed yet
            if (ioGameObject)
            {
                UnityEngine.Object.Destroy(ioGameObject);
            }

            ioGameObject = null;
        }

        /// <summary>
        /// Safely disposes of the parent GameObject of the transform and sets
        /// the reference to null.
        /// </summary>
        static public void SafeDestroyGO(ref UnityEngine.Transform ioTransform)
        {
            // This is to avoid calling Unity's overridden equality operator
            if (object.ReferenceEquals(ioTransform, null))
                return;

            // This is to see if the object hasn't been destroyed yet
            if (ioTransform && ioTransform.gameObject)
            {
                UnityEngine.Object.Destroy(ioTransform.gameObject);
            }

            ioTransform = null;
        }

        /// <summary>
        /// Safely disposes of the parent GameObject of the component and sets
        /// the reference to null.
        /// </summary>
        static public void SafeDestroyGO<T>(ref T ioComponent) where T : UnityEngine.Component
        {
            // This is to avoid calling Unity's overridden equality operator
            if (object.ReferenceEquals(ioComponent, null))
                return;

            // This is to see if the object hasn't been destroyed yet
            if (ioComponent && ioComponent.gameObject)
            {
                UnityEngine.Object.Destroy(ioComponent.gameObject);
            }

            ioComponent = null;
        }
    }
}
