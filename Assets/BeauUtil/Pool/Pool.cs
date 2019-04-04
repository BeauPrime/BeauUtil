using System;
using System.Diagnostics;

namespace BeauUtil
{
    /// <summary>
    /// Generic object pooler.
    /// </summary>
    public abstract partial class Pool<T> : IDisposable where T : class
    {
        /// <summary>
        /// Delegate for constructing an object with a reference
        /// to its pool.
        /// </summary>
        public delegate T Constructor(Pool<T> inPool);

        protected readonly Constructor m_Constructor;

        /// <summary>
        /// The current capacity of the pool.
        /// </summary>
        public abstract int Capacity { get; }

        /// <summary>
        /// Current count of objects within the pool.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Current count of objects outside the pool.
        /// </summary>
        public int InUse
        {
            get { return Capacity - Count; }
        }

        public Pool(Constructor inConstructor)
        {
            // Validate the type can actually be constructed.
            Type t = typeof(T);
            if (t.IsAbstract || t.IsInterface)
                throw new Exception("Cannot instantiate objects of the given type.");
            if (inConstructor == null)
                throw new ArgumentNullException();

            m_Constructor = inConstructor;
        }

        /// <summary>
        /// Disposes of the contents of the pool.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Resets the pool.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Returns the first free object.
        /// </summary>
        public abstract T Pop();

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        public abstract void Push(T inValue);

        /// <summary>
        /// Verifys that the object is valid.
        /// Only called if DEVELOPMENT is defined.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        protected void VerifyObject(T inObject)
        {
            if (inObject == null)
                throw new Exception("Null object provided for pool!");
            if (inObject.GetType().TypeHandle.Value != typeof(T).TypeHandle.Value)
                throw new Exception("Object of incorrect type provided for pool!");
        }
    }
}
