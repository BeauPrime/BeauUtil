/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    IUpdateVersioned.cs
 * Purpose: Versioned object interface, plus utility methods for checking for update changes.
 */

namespace BeauUtil
{
    /// <summary>
    /// Tracks version updates.
    /// </summary>
    public interface IUpdateVersioned
    {
        int GetUpdateVersion();
    }

    static public class UpdateVersion
    {
        public const int RESET_VALUE = -1;

        /// <summary>
        /// Increments the given version number.
        /// </summary>
        static public void Increment(ref int ioSerial)
        {
            if (ioSerial == int.MaxValue)
            {
                ioSerial = 0;
            }
            else
            {
                ++ioSerial;
            }
        }

        /// <summary>
        /// Resets the given version number.
        /// </summary>
        static public void Reset(ref int ioSerial)
        {
            ioSerial = RESET_VALUE;
        }

        /// <summary>
        /// Determines if the version numbers have changed.
        /// </summary>
        static public bool HasChanged(this IUpdateVersioned inVersioned, ref int ioSerial)
        {
            int currentSerial = inVersioned.GetUpdateVersion();
            if (ioSerial != currentSerial)
            {
                ioSerial = currentSerial;
                return true;
            }

            return false;
        }

        static public void Sync(this IUpdateVersioned inVersioned, ref int ioSerial)
        {
            HasChanged(inVersioned, ref ioSerial);
        }
    }
}