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

namespace Fap.Foundation.Sorting
{
    public class QuickSort<T> where T : IComparable
    {
        T[] input;

        public QuickSort(System.ComponentModel.BindingList<T> values)
        {
            input = new T[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                input[i] = values[i];
            }

        }

        public T[] Output
        {
            get
            {
                return input;
            }
        }
        public void Sort()
        {
            Sorting(0, input.Length - 1);
        }
        public int getPivotPoint(int begPoint, int endPoint)
        {
            int pivot = begPoint;
            int m = begPoint + 1;
            int n = endPoint;
            while ((m < endPoint) &&
                   (input[pivot].CompareTo(input[m]) >= 0))
            {
                m++;
            }

            while ((n > begPoint) &&
                   (input[pivot].CompareTo(input[n]) <= 0))
            {
                n--;
            }
            while (m < n)
            {
                T temp = input[m];
                input[m] = input[n];
                input[n] = temp;

                while ((m < endPoint) &&
                       (input[pivot].CompareTo(input[m]) >= 0))
                {
                    m++;
                }

                while ((n > begPoint) &&
                       (input[pivot].CompareTo(input[n]) <= 0))
                {
                    n--;
                }

            }
            if (pivot != n)
            {
                T temp2 = input[n];
                input[n] = input[pivot];
                input[pivot] = temp2;

            }
            return n;

        }
        public void Sorting(int beg, int end)
        {
            if (end == beg)
            {
                return;
            }
            else
            {
                int pivot = getPivotPoint(beg, end);
                if (pivot > beg)
                    Sorting(beg, pivot - 1);
                if (pivot < end)
                    Sorting(pivot + 1, end);
            }
        }
    }
}
