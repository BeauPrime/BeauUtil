/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 Oct 2022
 * 
 * File:    PredicateWithArg.cs
 * Purpose: Predicate delegate with passed-in context
 */

namespace BeauUtil
{
    /// <summary>
    /// Predicates that accepts an additional argument.
    /// </summary>
    public delegate bool Predicate<in TItem, in TArg>(TItem inItem, TArg inArg);
}