/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    15 Nov 2023
 * 
 * File:    IStateHash.cs
 * Purpose: Interface for an object with a state hash, plus utility methods for checking for changes.
 */

namespace BeauUtil
{
    /// <summary>
    /// Tracks state hash updates.
    /// </summary>
    public interface IStateHash
    {
        ulong GetStateHash();
    }

    static public class StateHash
    {
        /// <summary>
        /// Determines if the state hash has changed.
        /// </summary>
        static public bool HasChanged(this IStateHash inStateHash, ref ulong ioHash)
        {
            ulong currentHash = inStateHash.GetStateHash();
            if (ioHash != currentHash)
            {
                ioHash = currentHash;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the state hash has changed.
        /// </summary>
        static public bool HasChanged(ulong inStateHash, ref ulong ioHash)
        {
            if (ioHash != inStateHash)
            {
                ioHash = inStateHash;
                return true;
            }

            return false;
        }

        static public void Sync(this IStateHash inStateHash, ref ulong ioHash)
        {
            HasChanged(inStateHash, ref ioHash);
        }
    }
}