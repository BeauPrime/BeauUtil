/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    20 Nov 2019
 * 
 * File:    EditModeOnlyAttribute.cs
 * Purpose: Marks a property to be disabled when not in edit mode.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Disables this property if not in edit mode.
    /// </summary>
    public class EditModeOnlyAttribute : PropertyAttribute
    {
    }
}