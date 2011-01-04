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
