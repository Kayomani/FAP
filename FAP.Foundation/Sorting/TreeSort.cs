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
using System.Collections;

namespace Fap.Foundation.Sorting
{
    /// <summary>
    /// Custom tree sort which sorts into groups based on the ident string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeSort<T> : IEnumerable
    {
        private TreeSortNode<T> rootNode = new TreeSortNode<T>();
        private int count = 0;

        public int Count { get { return count; } }

        public void PutValue(string ident, T value)
        {
            rootNode.InsertValue(new List<char>(ident.ToCharArray()), value);
            count++;
        }

        public List<T> GetValue(string ident)
        {
            char[] keys = ident.ToCharArray();
            TreeSortNode<T> node = rootNode;
            for (int i = 0; i < keys.Length; i++)
            {
                bool found = false;
                for (int y = 0; y < node.SubValues.Count; y++)
                {
                    if (node.SubValues[y].Name == keys[i])
                    {
                        node = node.SubValues[y];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return new List<T>();
                }

            }
            if (null == node)
                return new List<T>();
            return node.Values;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            Stack<TreeSortNode<T>> stack = new Stack<TreeSortNode<T>>();

            stack.Push(rootNode);

            while (stack.Count > 0)
            {
                TreeSortNode<T> currentNode = stack.Pop();

                foreach (var subNode in currentNode.SubValues)
                    stack.Push(subNode);

                if (currentNode.Values.Count > 0)
                {
                    foreach (var value in currentNode.Values)
                        yield return value;
                }
            }
        }

        public List<TreeSortNode<T>> GetAllNodes()
        {
            List<TreeSortNode<T>> list = new List<TreeSortNode<T>>();

            Stack<TreeSortNode<T>> stack = new Stack<TreeSortNode<T>>();

            stack.Push(rootNode);

            while (stack.Count > 0)
            {
                TreeSortNode<T> currentNode = stack.Pop();

                foreach (var subNode in currentNode.SubValues)
                    stack.Push(subNode);

                if (currentNode.Values.Count > 0)
                    list.Add(currentNode);
            }

            return list;
        }
    }


    public class TreeSortNode<T>
    {
        public char Name { set; get; }
        private List<TreeSortNode<T>> subValues = null;
        private List<T> values = null;
        public List<T> Values
        {
            get
            {
                if (null == values)
                    values = new List<T>();
                return values;
            }
            set
            {
                if (value != null)
                {
                    if (value.Count != 0)
                    {
                        values = value;
                    }
                    else
                    {
                        values = null;
                    }
                }
                else
                {
                    values = null;
                }
            }
        }

        public List<TreeSortNode<T>> SubValues
        {
            get
            {
                if (null == subValues)
                    subValues = new List<TreeSortNode<T>>();
                return subValues;
            }
        }


        public void InsertValue(List<char> ident, T Value)
        {
            if (null == subValues)
                subValues = new List<TreeSortNode<T>>();
            if (ident.Count == 0)
            {
                Values.Add(Value);
                return;
            }
            bool match = false;
            for (int i = 0; i < subValues.Count; i++)
            {
                if (subValues[i].Name == ident[0])
                {
                    match = true;
                    ident.RemoveAt(0);
                    subValues[i].InsertValue(ident, Value);
                    break;
                }
            }
            if (!match)
            {
                TreeSortNode<T> newNode = new TreeSortNode<T>() { Name = ident[0] };
                subValues.Add(newNode);
                ident.RemoveAt(0);
                newNode.InsertValue(ident, Value);
            }
        }
    }
}
