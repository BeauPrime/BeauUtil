using System;
using System.Diagnostics;

namespace BeauUtil {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [Conditional("USING_TINYIL")]
    internal sealed class IntrinsicILAttribute : Attribute {
        public IntrinsicILAttribute(string instructions) {

        }
    }
}