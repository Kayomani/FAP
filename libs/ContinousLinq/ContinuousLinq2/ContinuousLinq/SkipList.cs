using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace ContinuousLinq
{
    public class SkipList<TKey, TValue>
    {
        public class SkipListNode
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }

            public SkipListNode Next { get; set; }

            public SkipListNode Below { get; set; }

            public SkipListNode()
            {
            }

            public SkipListNode(TKey key, TValue value)
            {
                this.Value = value;
                this.Key = key;
            }
        }

        private class HeaderSkipListNode : SkipListNode
        {
        }

        private int _currentLevel;
        private int _maxLevel;
        private Random _rand;

        SkipListNode _topLeft;
        SkipListNode _bottomLeft;

        Comparer<TKey> _comparer;

        public SkipList()
        {
            _currentLevel = 0;
            _maxLevel = 32;
            _rand = new Random();
            _comparer = Comparer<TKey>.Default;
            _topLeft = new HeaderSkipListNode();
            _bottomLeft = _topLeft;
        }

        public TValue GetValue(TKey key)
        {
            SkipListNode closestNode;
            if (!TryFindNodeForKey(key, out closestNode))
            {
                throw new KeyNotFoundException();
            }

            return closestNode.Value;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            SkipListNode closestNode;
            if (!TryFindNodeForKey(key, out closestNode))
            {
                value = default(TValue);
                return false;
            }
            value = closestNode.Value;
            return true;
        }

        private bool MatchesKey(TKey key, SkipListNode currentNode)
        {
            return !(currentNode is HeaderSkipListNode) && _comparer.Compare(key, currentNode.Key) == 0;
        }

        private bool TryFindNodeForKey(TKey key, out SkipListNode nodeMatchingKey)
        {
            bool isEqual;
            SkipListNode currentNode = GetClosestNode(key, out isEqual);

            if (isEqual)
            {
                nodeMatchingKey = currentNode;
                return true;
            }

            nodeMatchingKey = null;
            return false;
        }

        private SkipListNode GetClosestNode(TKey key, out bool isEqual)
        {
            var currentNode = SearchForwardLessThanEqual(key, _topLeft, out isEqual);
            while (currentNode.Below != null && !isEqual)
            {
                currentNode = currentNode.Below;
                currentNode = SearchForwardLessThanEqual(key, currentNode, out isEqual);
            }
            return currentNode;
        }

        private SkipListNode SearchForwardLessThanEqual(TKey key, SkipListNode startingNode, out bool isEqual)
        {
            var currentNode = startingNode;
            var currentNodeNext = currentNode.Next;
            int comparisonResult = -1;
            int lastComparisonResult = comparisonResult;
            while (currentNodeNext != null && (comparisonResult = _comparer.Compare(currentNodeNext.Key, key)) <= 0)
            {
                currentNode = currentNode.Next;
                currentNodeNext = currentNode.Next;
                lastComparisonResult = comparisonResult;
            }

            isEqual = lastComparisonResult == 0;

            return currentNode;
        }

        private SkipListNode SearchForwardLessThan(TKey key, SkipListNode startingNode)
        {
            var currentNode = startingNode;
            var currentNodeNext = currentNode.Next;
            while (currentNodeNext != null && _comparer.Compare(currentNodeNext.Key, key) < 0)
            {
                currentNode = currentNode.Next;
                currentNodeNext = currentNode.Next;
            }

            return currentNode;
        }

        public void Add(TKey key, TValue value)
        {
            SkipListNode newNodeBelow = AddRecursive(key, value, _topLeft);

            if (newNodeBelow != null &&
                _currentLevel < _maxLevel &&
                ShouldPromoteToNextLevel())
            {
                var newTopLeftNode = new HeaderSkipListNode();
                newTopLeftNode.Below = _topLeft;
                _topLeft = newTopLeftNode;

                InsertNewNode(key, value, newTopLeftNode, newNodeBelow);

                _currentLevel++;
            }
        }

        private SkipListNode AddRecursive(TKey key, TValue value, SkipListNode currentNode)
        {
            bool isEqual;
            var closestNodeAtThisLevel = SearchForwardLessThanEqual(key, currentNode, out isEqual);

            if (isEqual)
                throw new InvalidOperationException(string.Format("Key {0} already in collection", key));

            if (closestNodeAtThisLevel.Below != null)
            {
                var newNodeBelow = AddRecursive(key, value, closestNodeAtThisLevel.Below);

                if (newNodeBelow != null && ShouldPromoteToNextLevel())
                {
                    return InsertNewNode(key, value, closestNodeAtThisLevel, newNodeBelow);
                }
            }
            else
            {
                return InsertNewNode(key, value, closestNodeAtThisLevel);
            }

            return null;
        }

        private static SkipListNode InsertNewNode(TKey key, TValue value, SkipListNode nodeBefore)
        {
            SkipListNode newNode = new SkipListNode(key, value);
            newNode.Next = nodeBefore.Next;
            nodeBefore.Next = newNode;

            return newNode;
        }

        private static SkipListNode InsertNewNode(TKey key, TValue value, SkipListNode nodeBefore, SkipListNode belowForNewNode)
        {
            var newNode = InsertNewNode(key, value, nodeBefore);
            newNode.Below = belowForNewNode;
            return newNode;
        }

        private bool ShouldPromoteToNextLevel()
        {
            return (_rand.Next() & 1) == 0;
        }

        public void Remove(TKey key)
        {
            SkipListNode removedNode = RemoveRecursive(key, _topLeft);
            if (removedNode != null && _topLeft.Next == null && _topLeft.Below != null)
            {
                _topLeft = _topLeft.Below;
            }
        }

        private SkipListNode RemoveRecursive(TKey key, SkipListNode currentNode)
        {
            var closestNodeAtThisLevel = SearchForwardLessThan(key, currentNode);
            var nodeToRemove = closestNodeAtThisLevel.Next;

            if (closestNodeAtThisLevel.Below != null)
            {
                var removedNodeBelow = RemoveRecursive(key, closestNodeAtThisLevel.Below);

                if (removedNodeBelow != null &&
                    nodeToRemove != null &&
                    nodeToRemove.Below == removedNodeBelow)
                {
                    RemoveNode(nodeToRemove, closestNodeAtThisLevel);
                    return nodeToRemove;
                }
            }
            else if (nodeToRemove != null && MatchesKey(key, nodeToRemove))
            {
                RemoveNode(nodeToRemove, closestNodeAtThisLevel);
                return nodeToRemove;
            }

            return null;
        }

        private void RemoveNode(SkipListNode nodeToRemove, SkipListNode nodeBefore)
        {
            nodeBefore.Next = nodeToRemove.Next;
        }
    }
}
////Generic C# SkipList implementation                                                                       
////(C) Chris Lomont, 2009, www.lomont.org                                                                   
////Version 0.5, April 2009                                                                                  

////todo - add delegate in DEBUG for counting work done?                                                     
////todo - more contructors like SortedList<>                                                                
////todo - finish implementing some old-style collection interfaces                                          

//namespace Lomont
//{
//    /// <summary>                                                                                           
//    /// This class implements a skiplist, which is like a linked list                                       
//    /// but faster for finding items. It stores items in sorted order                                       
//    /// by Key.                                                                                             
//    /// </summary>                                                                                          
//    public class LomontSkipList<TKey, TValue> :
//        IDictionary<TKey, TValue>,
//        ICollection<KeyValuePair<TKey, TValue>>,
//        IEnumerable<KeyValuePair<TKey, TValue>>,
//        IEnumerable,
//        ICollection,
//        IDictionary
//        where TKey : IComparable<TKey>
//    {


//        /// <summary>                                                                                       
//        /// Default constructor                                                                             
//        /// </summary>                                                                                      
//        public LomontSkipList()
//        {
//            Reset();
//        }

//#if DEBUG                                                                                                   
//        /// <summary>                                                                                       
//        /// Dump state to Debug console                                                                     
//        /// </summary>                                                                                      
//        public void Dump()                                                                                  
//            {                                                                                               
//            Debug.WriteLine("");                                                                            
//            for (int i = 0; i < root.Level; ++i)                                                            
//                {                                                                                           
//                var ptr = root.Forward[i];                                                                  
//                while (ptr != null)                                                                         
//                    {                                                                                       
//                    Debug.Write(String.Format("{0} ",ptr.Value.ToString()));                                
//                    ptr = ptr.Forward[i];                                                                   
//                    }                                                                                       
//                Debug.WriteLine("");                                                                        
//                }                                                                                           
//            }                                                                                               
//#endif

//        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

//        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
//        {
//            SkipListNode<TKey, TValue> ptr = root.Forward[0];
//            while (ptr != null)
//            {
//                yield return new KeyValuePair<TKey, TValue>(ptr.Key, ptr.Value);
//                ptr = ptr.Forward[0];
//            }
//        }

//        #endregion

//        #region IEnumerable Members

//        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        #endregion

//        #region ICollection<KeyValuePair<TKey,TValue>> Members
//        /// <summary>                                                                                       
//        /// Adds an item to the collection                                                                  
//        /// </summary>                                                                                      
//        /// <param name="item"></param>                                                                     
//        public void Add(KeyValuePair<TKey, TValue> item)
//        {
//            Add(item.Key, item.Value);
//        }

//        /// <summary>                                                                                       
//        /// Removes all items from the collection                                                           
//        /// </summary>                                                                                      
//        public void Clear()
//        {
//            Reset();
//        }

//        /// <summary>                                                                                       
//        /// Determines whether the collection contains a specific value                                     
//        /// </summary>                                                                                      
//        /// <param name="item"></param>                                                                     
//        /// <returns></returns>                                                                             
//        public bool Contains(KeyValuePair<TKey, TValue> item)
//        {
//            TValue val;
//            bool foundKey = InternalFind(item.Key, out val);
//            return (foundKey) && (val.Equals(item.Value));
//        }

//        /// <summary>                                                                                       
//        /// Copies the elements of the collection to an array, starting at a particular array index         
//        /// </summary>                                                                                      
//        /// <param name="array"></param>                                                                    
//        /// <param name="arrayIndex"></param>                                                               
//        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
//        {
//            // todo - make better - can remove some copying                                                 
//            List<KeyValuePair<TKey, TValue>> lst = new List<KeyValuePair<TKey, TValue>>();
//            foreach (var n in this)
//                lst.Add(n);
//            lst.CopyTo(array, arrayIndex);
//        }

//        /// <summary>                                                                                       
//        /// Gets the number of elements contained in the collection                                         
//        /// </summary>                                                                                      
//        public int Count
//        {
//            get { return nodeCount; }
//        }

//        /// <summary>                                                                                       
//        /// Gets a value indicating whether the collection is read-only                                     
//        /// </summary>                                                                                      
//        public bool IsReadOnly
//        {
//            get { return false; }
//        }

//        /// <summary>                                                                                       
//        /// Removes the first occurrence of a specific object from the collection                           
//        /// </summary>                                                                                      
//        /// <param name="item"></param>                                                                     
//        /// <returns></returns>                                                                             
//        public bool Remove(KeyValuePair<TKey, TValue> item)
//        {
//            if (Contains(item))
//            {
//                InternalRemove(item.Key);
//                return true;
//            }
//            return false;
//        }

//        #endregion

//        #region IDictionary<TKey,TValue> Members

//        /// <summary>                                                                                       
//        /// Insert a (Key,Value) pair.                                                                      
//        /// Overwrites any pre-existing pair with a matching key.                                           
//        /// </summary>                                                                                      
//        /// <param name="searchKey"></param>                                                                
//        /// <param name="newValue"></param>                                                                 
//        public void Add(TKey searchKey, TValue newValue)
//        {
//            TValue temp;
//            if (searchKey == null) throw new ArgumentNullException();
//            if (InternalFind(searchKey, out temp)) throw new ArgumentException();
//            if (IsReadOnly) throw new NotSupportedException();
//            InternalAdd(searchKey, newValue);
//        }

//        /// <summary>                                                                                       
//        /// Determines whether the container contains an element with the specified key.                    
//        /// </summary>                                                                                      
//        /// <param name="key"></param>                                                                      
//        /// <returns></returns>                                                                             
//        public bool ContainsKey(TKey key)
//        {
//            TValue temp;
//            return InternalFind(key, out temp);
//        }

//        /// <summary>                                                                                       
//        /// Gets an ICollection containing the keys of the IDictionary.                                     
//        /// </summary>                                                                                      
//        public ICollection<TKey> Keys
//        {
//            // todo - speed up? cache?                                                                      
//            get
//            {
//                List<TKey> keys = new List<TKey>();
//                foreach (var n in this)
//                    keys.Add(n.Key);
//                return keys;
//            }
//        }

//        /// <summary>                                                                                       
//        /// Gets an ICollection containing the keys of the IDictionary.                                     
//        /// </summary>                                                                                      
//        /// <param name="key"></param>                                                                      
//        /// <returns></returns>                                                                             
//        bool IDictionary<TKey, TValue>.Remove(TKey key)
//        {
//            return this.InternalRemove(key);
//        }

//        /// <summary>                                                                                       
//        /// Gets the value associated with the specified key.                                               
//        /// </summary>                                                                                      
//        /// <param name="key">The key whose value to get.</param>                                           
//        /// <param name="value">When this method returns, the value associated                              
//        /// with the specified key, if the key is found; otherwise, the default                             
//        /// value for the type of the value parameter. This parameter is passed                             
//        /// uninitialized.</param>                                                                          
//        /// <returns></returns>                                                                             
//        public bool TryGetValue(TKey key, out TValue value)
//        {
//            return InternalFind(key, out value);
//        }

//        /// <summary>                                                                                       
//        /// Gets an ICollection containing the values in the IDictionary.                                   
//        /// </summary>                                                                                      
//        public ICollection<TValue> Values
//        {
//            // todo - speed up? cache?                                                                      
//            get
//            {
//                List<TValue> values = new List<TValue>();
//                foreach (var n in this)
//                    values.Add(n.Value);
//                return values;
//            }
//        }

//        /// <summary>                                                                                       
//        /// Gets or sets the element with the specified key.                                                
//        /// </summary>                                                                                      
//        /// <param name="key"></param>                                                                      
//        /// <returns></returns>                                                                             
//        public TValue this[TKey key]
//        {
//            get
//            {
//                TValue temp;
//                if (key == null) throw new ArgumentNullException();
//                if (!InternalFind(key, out temp)) throw new KeyNotFoundException();
//                return temp;
//            }
//            set
//            {
//                if (key == null) throw new ArgumentNullException();
//                if (IsReadOnly) throw new NotSupportedException();
//                Add(key, value);
//            }
//        }

//        #endregion

//        #region ICollection Members

//        public void CopyTo(Array array, int index)
//        { // todo - implement? use generic version                                                      
//            throw new NotImplementedException();
//        }

//        /// <summary>                                                                                       
//        /// Gets a value indicating whether access to the ICollection is synchronized (thread safe).        
//        /// </summary>                                                                                      
//        public bool IsSynchronized
//        {
//            get { return false; }
//        }

//        object lockObj = new object();
//        /// <summary>                                                                                       
//        /// Gets an object that can be used to synchronize access to the collection.                        
//        /// </summary>                                                                                      
//        public object SyncRoot
//        {
//            get { return lockObj; }
//        }

//        #endregion

//        #region IDictionary Members
//        // todo -imeplement these? for now use generic ones                                                 

//        public void Add(object key, object value)
//        {
//            throw new NotImplementedException();
//        }

//        public bool Contains(object key)
//        {
//            throw new NotImplementedException();
//        }

//        IDictionaryEnumerator IDictionary.GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }

//        public bool IsFixedSize
//        {
//            get { throw new NotImplementedException(); }
//        }

//        ICollection IDictionary.Keys
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public void Remove(object key)
//        {
//            throw new NotImplementedException();
//        }

//        ICollection IDictionary.Values
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public object this[object key]
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        #endregion

//        #region Implementation
//        /// <summary>                                                                                       
//        /// Reset internals to empty collection                                                             
//        /// </summary>                                                                                      
//        void Reset()
//        {
//            root = new LomontSkipList<TKey, TValue>.SkipListNode<TKey, TValue>(default(TKey), default(TValue), 1);
//            nodeCount = 0;
//        }


//        /// <summary>                                                                                       
//        /// The count of nodes in the (Key, Value) pairs in the container                                   
//        /// </summary>                                                                                      
//        int nodeCount = 0;

//        /// <summary>                                                                                       
//        /// A single skiplist node that stores                                                              
//        /// (Key,Value) pairs                                                                               
//        /// </summary>                                                                                      
//        /// <typeparam name="T"></typeparam>                                                                
//        class SkipListNode<TKeyNode, TValueNode>
//        {
//            /// <summary>                                                                                   
//            /// Level is the depth of this node, i.e.,                                                      
//            /// the number of pointers going forward                                                        
//            /// to higher valued nodes                                                                      
//            /// </summary>                                                                                  
//            public int Level { get { return Forward.Length; } }
//            /// <summary>                                                                                   
//            /// Forward are the forward pointers for each skip depth                                        
//            /// </summary>                                                                                  
//            public SkipListNode<TKeyNode, TValueNode>[] Forward;
//            /// <summary>                                                                                   
//            /// Key value used for lookup                                                                   
//            /// </summary>                                                                                  
//            public TKeyNode Key;
//            /// <summary>                                                                                   
//            /// Value stored under a given key                                                              
//            /// </summary>                                                                                  
//            public TValueNode Value;

//            /// <summary>                                                                                   
//            /// Construct a new node with given (Key,Value) amount and                                      
//            /// specified depth (# of forward pointers)                                                     
//            /// </summary>                                                                                  
//            /// <param name="key"></param>                                                                  
//            /// <param name="value"></param>                                                                
//            /// <param name="depth"></param>                                                                
//            public SkipListNode(TKeyNode key, TValueNode value, int depth)
//            {
//                Key = key;
//                Value = value;
//                Forward = new SkipListNode<TKeyNode, TValueNode>[depth];
//            }
//        }


//        /// <summary>                                                                                       
//        /// The first node in the list, before any that contain (Key,Value) pairs.                          
//        /// All user nodes come after this one. Each list is ended with a null.                             
//        /// </summary>                                                                                      
//        SkipListNode<TKey, TValue> root;

//        /// <summary>                                                                                       
//        /// Find the value associated with the given key.                                                   
//        /// Return true if found, else false and                                                            
//        /// default value (usually null or 0) if not found.                                                 
//        /// </summary>                                                                                      
//        /// <param name="key"></param>                                                                      
//        /// <returns></returns>                                                                             
//        bool InternalFind(TKey searchKey, out TValue val)
//        {
//            SkipListNode<TKey, TValue> x = root;
//            // walk forward, keeping x < searchKey                                                          
//            for (int i = root.Level - 1; i >= 0; --i)
//            {
//                while ((x.Forward[i] != null) && (x.Forward[i].Key.CompareTo(searchKey) < 0))
//                    x = x.Forward[i];
//            }
//            // either next entry is x value or x not present                                                
//            x = x.Forward[0];
//            if ((x != null) && (x.Key.CompareTo(searchKey) == 0))
//            {
//                val = x.Value;
//                return true;
//            }
//            val = default(TValue);
//            return false;
//        }

//        /// <summary>                                                                                       
//        /// Insert a (Key,Value) pair.                                                                      
//        /// Overwrites any pre-existing pair with a matching key.                                           
//        /// </summary>                                                                                      
//        /// <param name="searchKey"></param>                                                                
//        /// <param name="newValue"></param>                                                                 
//        void InternalAdd(TKey searchKey, TValue newValue)
//        {
//            SkipListNode<TKey, TValue>[] update = new SkipListNode<TKey, TValue>[root.Level];
//            SkipListNode<TKey, TValue> x = root;
//            for (int i = root.Level - 1; i >= 0; --i)
//            {
//                while ((x.Forward[i] != null) && (x.Forward[i].Key.CompareTo(searchKey) < 0))
//                    x = x.Forward[i];
//                // update[i] holds the node with key >= x.key at each depth i                               
//                update[i] = x;
//            }
//            x = x.Forward[0]; // now x is null or x.Value >= searchKey. Insert here                         
//            Debug.Assert((x == null) || (x.Key.CompareTo(searchKey) >= 0));
//            if ((x != null) && (x.Key.CompareTo(searchKey) == 0))
//                x.Value = newValue; // overwrite existing value                                             
//            else
//            { // insert node                                                                            
//                nodeCount++;
//                int lvl = RandomLevel();
//                if (lvl > root.Level)
//                {
//                    // lengthen root.Forward and local variable update to new length.                       
//                    // new update entries must point to root                                                
//                    Array.Resize(ref update, lvl);
//                    for (int i = root.Level; i < lvl; ++i)
//                        update[i] = root;

//                    Array.Resize(ref root.Forward, lvl);
//                }
//                x = new LomontSkipList<TKey, TValue>.SkipListNode<TKey, TValue>(searchKey, newValue, lvl);
//                for (int i = 0; i < lvl; i++)
//                {
//                    x.Forward[i] = update[i].Forward[i];
//                    update[i].Forward[i] = x;
//                }
//            }
//        } // Add                                                                                        

//        /// <summary>                                                                                       
//        /// Delete the (Key,Value) pair with the given Key value                                            
//        /// if it exists, else do nothing.                                                                  
//        /// Return true if it was present and removed.                                                      
//        /// </summary>                                                                                      
//        /// <param name="searchKey"></param>                                                                
//        bool InternalRemove(TKey searchKey)
//        {
//            SkipListNode<TKey, TValue>[] update = new SkipListNode<TKey, TValue>[root.Level];
//            SkipListNode<TKey, TValue> x = root;
//            for (int i = root.Level - 1; i >= 0; --i)
//            {
//                while ((x.Forward[i] != null) && (x.Forward[i].Key.CompareTo(searchKey) < 0))
//                    x = x.Forward[i];
//                // update[i] holds the node with key >= x.key at each depth i                               
//                update[i] = x;
//            }
//            x = x.Forward[0];  // now x is null or x.Value >= searchKey. Delete here                        
//            Debug.Assert((x == null) || (x.Key.CompareTo(searchKey) >= 0));
//            if (x.Key.CompareTo(searchKey) == 0)
//            { // found , remove node                                                                    
//                nodeCount--;
//                for (int i = 0; i < root.Level; ++i)
//                {
//                    if (update[i].Forward[i] != x)
//                        break;
//                    update[i].Forward[i] = x.Forward[i];
//                }
//                // shrink root if needed                                                                    
//                int last = root.Level - 1;
//                while ((last > 0) && (root.Forward[last] == null))
//                    last--;
//                if (last + 1 != root.Level)
//                    Array.Resize(ref root.Forward, last + 1);
//                return true;
//            }
//            return false;
//        } // Remove                                                                                     

//        /// <summary>                                                                                       
//        /// Random generator for obtaining a new level                                                      
//        /// </summary>                                                                                      
//        Random rand = new Random();

//        /// <summary>                                                                                       
//        /// Probability of making deeper level - todo explain                                               
//        /// </summary>                                                                                      
//        static double prob = 0.5; // todo - make tunable from outside?                                      

//        /// <summary>                                                                                       
//        /// Obtain a random level depth                                                                     
//        /// </summary>                                                                                      
//        /// <returns></returns>                                                                             
//        int RandomLevel()
//        {
//            int level = 1; // new nodes need at least 1 forward pointer.                                    
//            // todo - Note: we implement level cap this way to prevent runaway trees, as mentioned in       
//            // Pughs original paper. Test this cap! (try +1, +2, etc). Limits rapid growth                  
//            while ((rand.NextDouble() < prob) && (level < root.Level + 1))
//                ++level;
//            return level;
//        }
//        #endregion
//    }
//}
