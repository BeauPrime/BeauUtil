/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
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