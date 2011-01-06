using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Management;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Enumerator wrapper for enumerating WMI object in the collection
    /// it takes an ManagementObjectCollection and outputs our user defined types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WmiObjectEnumerator<T> : IEnumerator<T> where T : new()
    {

        ManagementObjectCollection _collection = null;
        IEnumerator _enumerator = null;

        public WmiObjectEnumerator(ManagementObjectCollection collection)
        {
            _collection = collection;
            _enumerator = collection.GetEnumerator();
            Reset();
        }

        public T Current
        {
            get
            {
                return (T)WmiMapper.MapToEntity<T>((ManagementObject)_enumerator.Current);
            }
        }

        /// <summary>
        /// Clean up the resources
        /// </summary>
        public void Dispose()
        {
            _collection.Dispose();
        }

        /// <summary>
        /// Retrieves current entity
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        /// <summary>
        /// Moves to next entity
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        /// <summary>
        /// Resets the enumerator
        /// </summary>
        public void Reset()
        {
            _enumerator.Reset();
        }
    }
}
