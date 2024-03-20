using System;
using System.Diagnostics;

namespace BeauUtil {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [Conditional("USING_TINYIL")]
    internal sealed class ExternalILAttribute : Attribute {
        public ExternalILAttribute(string filePatchName) {

        }
    }
}