#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Fap.Foundation
{
    public class SafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly object syncRoot = new object();
        private Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();

        #region IDictionary<TKey,TValueMembers

        public void Add(TKey key, TValue value)
        {
            lock (syncRoot)
            {
                d.Add(key, value);
            }
        }

        public void Set(TKey key, TValue value)
        {
            lock (syncRoot)
            {
                if (d.ContainsKey(key))
                    d[key] = value;
                else
                  d.Add(key, value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (syncRoot)
                return d.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (syncRoot)
                {
                    return d.Keys;
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (syncRoot)
            {
                return d.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (syncRoot)
            {
                return d.TryGetValue(key, out value);
            }
        }

        public TValue SafeGet(TKey k)
        {
            lock (syncRoot)
            {
                if (d.ContainsKey(k))
                    return d[k];
                return default(TValue);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (syncRoot)
                {
                    return d.Values;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return d[key];
            }
            set
            {
                lock (syncRoot)
                {
                    d[key] = value;
                }
            }
        }

        #endregion


        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)d).Add(item);
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                d.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey,
            TValue>>)d).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int
        arrayIndex)
        {
            lock (syncRoot)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)d).CopyTo(array,
                arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                return d.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                return ((ICollection<KeyValuePair<TKey,
                TValue>>)d).Remove(item);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)d).GetEnumerator();
        }


        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)d).GetEnumerator();
        }

    }
}
