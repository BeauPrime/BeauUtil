/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    27 Sept 2019
 * 
 * File:    OrderAttribute.cs
 * Purpose: Specifies an order for the given element.
 */

using System;

namespace BeauUtil
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class OrderAttribute : Attribute
    {
        public int Order { get; private set; }
        
        public OrderAttribute(int inOrder)
        {
            Order = inOrder;
        }
    }
}