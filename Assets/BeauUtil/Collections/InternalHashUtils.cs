using System.Reflection;
using System;
using System.Collections;

namespace BeauUtil
{
    static internal class InternalHashUtils
    {
        #region Hash Helpers

        static private readonly Func<int, int> HashHelper_GetPrime;
        static private readonly Func<int, int> HashHelper_ExpandPrime;
        static private readonly Func<int, bool> HashHelper_IsPrime;

        static InternalHashUtils()
        {
            Type hashHelpers = Assembly.GetAssembly(typeof(IEnumerator)).GetType("System.Collections.HashHelpers");
            if (hashHelpers != null)
            {
                HashHelper_GetPrime = (Func<int, int>) hashHelpers.GetMethod("GetPrime", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.CreateDelegate(typeof(Func<int, int>));
                HashHelper_ExpandPrime = (Func<int, int>) hashHelpers.GetMethod("ExpandPrime", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.CreateDelegate(typeof(Func<int, int>));
                HashHelper_IsPrime = (Func<int, bool>) hashHelpers.GetMethod("IsPrime", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.CreateDelegate(typeof(Func<int, bool>));
            }
        }

        /// <summary>
        /// Returns the next prime number
        /// </summary>
        static internal int GetPrime(int inSize)
        {
            return HashHelper_GetPrime?.Invoke(inSize) ?? inSize;
        }

        /// <summary>
        /// Returns the next prime number
        /// </summary>
        static internal int ExpandPrime(int inSize)
        {
            return HashHelper_ExpandPrime?.Invoke(inSize) ?? inSize;
        }

        /// <summary>
        /// Returns if the given value is prime.
        /// </summary>
        static internal bool IsPrime(int inSize)
        {
            return HashHelper_IsPrime?.Invoke(inSize) ?? false;
        }

        #endregion // Hash Helpers
    }
}