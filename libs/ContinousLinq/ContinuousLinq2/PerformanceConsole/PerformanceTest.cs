using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ContinuousLinq;
using ContinuousLinq.Aggregates;
using ContinuousLinq.Expressions;
using System.Threading;
using System.ComponentModel;

namespace PerformanceConsole
{
    public class PerformanceTest
    {
        ObservableCollection<Person> _source;

        public PerformanceTest()
        {
            _source = new ObservableCollection<Person>();

            for (int i = 0; i < 3000; i++)
            {
                _source.Add(new Person(i.ToString(), i));
            }
        }
        public void TakeTest()
        {
            DateTime start = DateTime.Now;
            var result = _source.Take(500);
            for (int i = _source.Count; i > 0; i--)
            {
                _source.RemoveAt(0);
            }
            var duration = DateTime.Now - start;
            Console.WriteLine(duration.TotalMilliseconds + " ms");
            start = DateTime.Now;
            for (int i = 0; i < 3000; i++)
            {
                _source.Add(new Person(i.ToString(), i));
            }
            duration = DateTime.Now - start;
            Console.WriteLine(duration.TotalMilliseconds + " ms");
        }

        public void SkipTest()
        {
            DateTime start = DateTime.Now;
            var result = _source.Skip(500);
            for (int i = _source.Count; i > 0; i--)
            {
                _source.RemoveAt(0);
            }
            var duration = DateTime.Now - start;
            Console.WriteLine(duration.TotalMilliseconds+" ms");
            start = DateTime.Now;
            for(int i = 0; i<3000; i++)
            {
                _source.Add(new Person(i.ToString(), i));
            }
            duration = DateTime.Now - start;
            Console.WriteLine(duration.TotalMilliseconds + " ms");  
        }
        public void SkipTakeCombineAssertTest()
        {
            DateTime start = DateTime.Now;

            //var result = (from p in _source
            //             where p.Age % 3 == 0
            //             select p).Skip(100).Take(45);
            //System.Diagnostics.Debug.Assert(result[0].Age==300);
            //System.Diagnostics.Debug.Assert(result.Count == 45);
            //int countIndex = 0;
            //start = DateTime.Now;
            //foreach(int num in result.Select(p=>p.Age))
            //{
            //    System.Diagnostics.Debug.Assert((countIndex + 100)*3 == num);
            //    countIndex++;
            //}
            //System.Diagnostics.Debug.Assert(countIndex == 45);
            //var duration = DateTime.Now - start;
            //Console.WriteLine(duration.TotalMilliseconds + " ms");
            var result = (from p in
                              ((from p in _source
                                select p).Skip(100).Take(45))
                          where p.Age % 3 == 0
                          select p).OrderBy(i => i.Age);

            System.Diagnostics.Debug.Assert(result[0].Age == 102);
            System.Diagnostics.Debug.Assert(result.Count == 15);
            int countIndex = 0;
            start = DateTime.Now;
            foreach (int num in result.Select(p => p.Age))
            {
                System.Diagnostics.Debug.Assert((countIndex + 34) * 3 == num);
                countIndex++;
            }
            _source.Insert(101, new Person("", 2));
            _source.Insert(105, new Person("", 33));
            countIndex = 0;
            System.Diagnostics.Debug.Assert(result[0].Age==33);
            foreach (int num in result.Select(p => p.Age).Skip(1))
            {
                System.Diagnostics.Debug.Assert((countIndex + 34) * 3 == num);
                countIndex++;
            }
            System.Diagnostics.Debug.Assert(countIndex == 14);
            var duration = DateTime.Now - start;
            Console.WriteLine(duration.TotalMilliseconds + " ms");
        }
        public void WhereTest()
        {
            Random rand = new Random();

            var result = from p in _source
                         where p.Age > 1500
                         select p;

            DateTime start = DateTime.Now;

            for (int i = 0; i < 1000000; i++)
            {
                int index = rand.Next(_source.Count);
                _source[index].Age = _source[index].Age > 1500 ? 0 : 1501;
            }

            var duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }


        public void SelectTest()
        {
            Random rand = new Random();

            var result = from p in _source
                         select p.Age;

            DateTime start = DateTime.Now;

            for (int i = 0; i < 1000000; i++)
            {
                int index = rand.Next(_source.Count);
                _source[index].Age = _source[index].Age > 1500 ? 0 : 1501;

                if (_source[index].Age != result[index])
                    throw new Exception();
            }

            var duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }


        public void SelectLinearUpdateTest()
        {
            Random rand = new Random();

            var result = from p in _source
                         select p.Age;

            DateTime start = DateTime.Now;

            int updateIndex = 0;
            for (int i = 0; i < 1000000; i++)
            {
                if (updateIndex >= _source.Count)
                    updateIndex = 0;

                _source[updateIndex].Age++;

                if (_source[updateIndex].Age != result[updateIndex])
                    throw new Exception();

                updateIndex++;
            }

            var duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }


        public void SelectUnrelatedPropertyLinearUpdateTest()
        {
            Random rand = new Random();

            var result = from p in _source
                         select p;

            DateTime start = DateTime.Now;

            int updateIndex = 0;
            for (int i = 0; i < 1000000; i++)
            {
                if (updateIndex >= _source.Count)
                    updateIndex = 0;

                _source[updateIndex].Age++;

                if (_source[updateIndex].Age != result[updateIndex].Age)
                    throw new Exception();

                updateIndex++;
            }

            var duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }


        public void ContinuousSumWithoutPausing()
        {
            Random rand = new Random();

            DateTime start = DateTime.Now;

            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);

            int updateIndex = 0;
            for (int i = 0; i < 10000; i++)
            {
                if (updateIndex >= _source.Count)
                    updateIndex = 0;

                _source[updateIndex].Age++;

                if (sum.CurrentValue <= 0)
                    throw new Exception();

                updateIndex++;
            }

            var duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }


        public void ContinuousSumWithPausing()
        {
            Random rand = new Random();

            DateTime start = DateTime.Now;

            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);

            using (PausedAggregation pausedAggregation = new PausedAggregation())
            {
                int updateIndex = 0;
                for (int i = 0; i < 10000; i++)
                {
                    if (updateIndex >= _source.Count)
                        updateIndex = 0;

                    _source[updateIndex].Age++;
                    if (sum.CurrentValue <= 0)
                        throw new Exception();

                    updateIndex++;
                }
            }

            var duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }

        public void TestDynamicInvoke()
        {
            Random rand = new Random();

            int a = 0;
            Action del = () => { a++; };
            Delegate baseDelegate = del;

            TimeSpan duration;
            DateTime start;

            start = DateTime.Now;

            for (int i = 0; i < 10000; i++)
            {
                del();
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());

            start = DateTime.Now;

            for (int i = 0; i < 10000; i++)
            {
                baseDelegate.DynamicInvoke();
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }

        public void TestDynamicProperty()
        {
            Random rand = new Random();
            Person person = new Person();
            person.Brother = new Person();

            TimeSpan duration;
            DateTime start;

            start = DateTime.Now;

            for (int i = 0; i < 100000; i++)
            {
                Person brother = person.Brother;
                brother.Age++;
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());

            DynamicProperty brotherProperty = DynamicProperty.Create(typeof(Person), "Brother");

            start = DateTime.Now;

            for (int i = 0; i < 100000; i++)
            {
                Person brother = (Person)brotherProperty.GetValue(person);
                brother.Age++;
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }

        public void SortingTest()
        {
            Random rand = new Random();
            int ITEMS = 3000;
            int MAX = ITEMS * 4;
            int[] data = new int[ITEMS];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = rand.Next(MAX);
            }

            TimeSpan duration;
            DateTime start;

            start = DateTime.Now;

            Array.Sort(data);

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());

            data = new int[ITEMS];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = rand.Next(MAX);
            }

            start = DateTime.Now;

            QuickSort(data, 0, data.Length - 1);

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }

        private int[] QuickSort(int[] a, int i, int j)
        {
            if (i < j)
            {
                int q = Partition(a, i, j);
                a = QuickSort(a, i, q);
                a = QuickSort(a, q + 1, j);
            }
            return a;
        }

        private int Partition(int[] a, int p, int r)
        {
            int x = a[p];
            int i = p - 1;
            int j = r + 1;
            int tmp = 0;
            while (true)
            {
                do
                {
                    j--;
                } while (a[j] > x);
                do
                {
                    i++;
                } while (a[i] < x);
                if (i < j)
                {
                    tmp = a[i];
                    a[i] = a[j];
                    a[j] = tmp;
                }
                else return j;
            }
        }

        private static List<T> CreateListOfPeople<T>(int count) where T : new()
        {
            List<T> innerList = new List<T>();
            for (int j = 0; j < count; j++)
            {
                innerList.Add(new T());
            }
            return innerList;
        }

        private List<List<Person>> _people;
        private List<List<NotifyingPerson>> _notifyingPeople;

        private List<ReadOnlyContinuousCollection<Person>> _peopleQueries;
        private List<ReadOnlyContinuousCollection<NotifyingPerson>> _notifyingPeopleQueries;


        public void CreatePeople(int OUTER_LIST_COUNT, int INNER_LIST_COUNT)
        {
            _people = new List<List<Person>>();

            for (int i = 0; i < OUTER_LIST_COUNT; i++)
            {
                List<Person> innerList = CreateListOfPeople<Person>(INNER_LIST_COUNT);
                _people.Add(innerList);
            }
        }

        private void CreateQueries(int OUTER_LIST_COUNT, int INNER_LIST_COUNT)
        {
            _peopleQueries = new List<ReadOnlyContinuousCollection<Person>>();
            ObservableCollection<Person> innerList = new ObservableCollection<Person>();
            for (int j = 0; j < INNER_LIST_COUNT; j++)
            {
                innerList.Add(new Person());
            }

            for (int i = 0; i < OUTER_LIST_COUNT; i++)
            {
                var query = from person in innerList
                            where person.Age > 10
                            select person;

                _peopleQueries.Add(query);
            }
        }

        public void CreateNotifyingPeople(int OUTER_LIST_COUNT, int INNER_LIST_COUNT)
        {
            _notifyingPeople = new List<List<NotifyingPerson>>();

            for (int i = 0; i < OUTER_LIST_COUNT; i++)
            {
                List<NotifyingPerson> innerList = CreateListOfPeople<NotifyingPerson>(INNER_LIST_COUNT);
                _notifyingPeople.Add(innerList);
            }
        }

        private void CreateNotifyingQueries(int OUTER_LIST_COUNT, int INNER_LIST_COUNT)
        {
            _notifyingPeopleQueries = new List<ReadOnlyContinuousCollection<NotifyingPerson>>();
            ObservableCollection<NotifyingPerson> innerList = new ObservableCollection<NotifyingPerson>();
            for (int j = 0; j < INNER_LIST_COUNT; j++)
            {
                innerList.Add(new NotifyingPerson());
            }

            for (int i = 0; i < OUTER_LIST_COUNT; i++)
            {
                var query = from person in innerList
                            where person.Age > 10
                            select person;

                _notifyingPeopleQueries.Add(query);
            }
        }

        public void MemoryTest()
        {
            const int OUTER_LIST_COUNT = 500;
            const int INNER_LIST_COUNT = 1000;

            //long totalMemoryBase;
            //Thread.MemoryBarrier();
            //totalMemoryBase = GC.GetTotalMemory(true);
            //Console.WriteLine("TotalMemoryBase: {0}", totalMemoryBase);
            //DoCompleteCollect();

            //CreatePeople(OUTER_LIST_COUNT, INNER_LIST_COUNT);

            //Thread.MemoryBarrier(); 
            //long memoryForJustLists;
            //memoryForJustLists = GC.GetTotalMemory(true) - totalMemoryBase;
            //Console.WriteLine("Simple Lists: {0}", memoryForJustLists);

            //_people = null;

            //Thread.MemoryBarrier();
            //long memoryAfterClean = GC.GetTotalMemory(true) - totalMemoryBase;
            //Console.WriteLine("Memory after clean: {0}", memoryAfterClean);
            //totalMemoryBase = GC.GetTotalMemory(true);

            DoCompleteCollect();
            Console.ReadLine();
            CreateNotifyingQueries(OUTER_LIST_COUNT, INNER_LIST_COUNT);
            Console.WriteLine("Created");
            DoCompleteCollect();
            Console.ReadLine();
            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine(_notifyingPeopleQueries.Count);
            }
            //long memoryForQueries;
            //Thread.MemoryBarrier();
            //memoryForQueries = GC.GetTotalMemory(true) - totalMemoryBase;
            //Console.WriteLine("Queries: {0}", memoryForJustLists);

            //DoCompleteCollect();
            //Console.ReadLine();
        }

        public void CompareQueryCreation()
        {
            const int OUTER_LIST_COUNT = 1000;
            const int INNER_LIST_COUNT = 300;

            TimeSpan duration;
            DateTime start;

            start = DateTime.Now;

            CreateQueries(OUTER_LIST_COUNT, INNER_LIST_COUNT);

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());

            start = DateTime.Now;

            CreateNotifyingQueries(OUTER_LIST_COUNT, INNER_LIST_COUNT);

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }

        private static void DoCompleteCollect()
        {
            Thread.MemoryBarrier();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void GetInterfaceTest()
        {
            const int ITERATIONS = 1000000;
            bool result = false;
            TimeSpan duration;
            DateTime start;

            start = DateTime.Now;

            for (int i = 0; i < ITERATIONS; i++)
            {
                result = typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(Person));
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());

            start = DateTime.Now;

            for (int i = 0; i < ITERATIONS; i++)
            {
                result = typeof(Person).GetInterface(typeof(INotifyPropertyChanged).Name) != null;
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
            Console.WriteLine(result);
        }

        private void TimeIt(int numberOfTimes, Action action)
        {
            TimeSpan duration;
            DateTime start;

            start = DateTime.Now;

            for (int i = 0; i < numberOfTimes; i++)
            {
                action();
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration.ToString());
        }

        Stack<int> _recursiveStack = new Stack<int>();

        public void RecursiveFunctionVsStack()
        {
            const int trials = 1000000;
            TimeIt(trials, () =>
            {
                const int iterations = 20;

                for (int i = 0; i < iterations; i++)
                {
                    _recursiveStack.Push(i);
                }

                for (int i = 0; i < iterations; i++)
                {
                    _lastUpdated = _recursiveStack.Pop();
                }
            });

            TimeIt(trials, () =>
            {
                int foo;
                RecursiveCall(0, out foo);
            });
        }

        int _lastUpdated = 0;
        private void RecursiveCall(int i, out int foo)
        {
            if (i < 20)
            {
                foo = 20;
                return;
            }

            _lastUpdated = i;

            RecursiveCall(i++, out foo);
        }

        private static int[] CreateRandomSetOfKeys(int items)
        {
            Random rand = new Random();

            int[] keys = new int[items];
            for (int i = 0; i < items; i++)
            {
                keys[i] = i;
            }

            for (int i = 0; i < items; i++)
            {
                int firstIndex = rand.Next(items);
                int secondIndex = rand.Next(items);
                int temp = keys[firstIndex];
                keys[firstIndex] = keys[secondIndex];
                keys[secondIndex] = temp;
            }
            return keys;
        }

        const int TRIALS = 1;
        const int ITEMS = 1000000;

        public void SkipListVsSortedDictionaryAdds()
        {
            Console.WriteLine("SkipListVsSortedDictionaryAdds");

            int[] keys = CreateRandomSetOfKeys(ITEMS);
            Console.WriteLine("SkipList");
            //Console.ReadLine();
            SkipList<int, int> skipList = new SkipList<int, int>();
            TimeIt(TRIALS, () =>
            {
                for (int i = 0; i < ITEMS; i++)
                {
                    skipList.Add(keys[i], i);
                }
            });

            Console.WriteLine("SortedDictionary");
            SortedDictionary<int, int> sortedDictionary = new SortedDictionary<int, int>();
            TimeIt(TRIALS, () =>
            {
                for (int i = 0; i < ITEMS; i++)
                {
                    sortedDictionary.Add(keys[i], i);
                }
            });
        }

        public void SkipListVsSortedDictionaryLookups()
        {
            Console.WriteLine("SkipListVsSortedDictionaryLookups");
            int[] keys = CreateRandomSetOfKeys(ITEMS);

            Console.WriteLine("SortedDictionary");
            SortedDictionary<int, int> sortedDictionary = new SortedDictionary<int, int>();
            for (int i = 0; i < ITEMS; i++)
            {
                sortedDictionary.Add(keys[i], i);
            }

            TimeIt(TRIALS, () =>
            {
                int val;
                for (int i = 0; i < ITEMS; i++)
                {
                    val = sortedDictionary[keys[i]];
                }
            });

            Console.WriteLine("SkipList");
            SkipList<int, int> skipList = new SkipList<int, int>();
            for (int i = 0; i < ITEMS; i++)
            {
                skipList.Add(keys[i], i);
            }

            TimeIt(TRIALS, () =>
            {
                int val;
                for (int i = 0; i < ITEMS; i++)
                {
                    val = skipList.GetValue(keys[i]);
                }
            });
        }

        public void GroupJoin()
        {
            Random rand = new Random();

            //_standardLinqResults = from outerPerson in _outer.AsEnumerable()
            //                       join innerPerson in _inner on outerPerson.Age equals innerPerson.Age into innersMatchingOuterAge
            //                       select new Pair<Person, IEnumerable<Person>>(outerPerson, innersMatchingOuterAge);
            Console.WriteLine("Ready");
            Console.ReadLine();

            int outerItems = 500;
            int innerItems = 7000;

            ContinuousCollection<NotifyingPerson> inner = new ContinuousCollection<NotifyingPerson>();
            ContinuousCollection<NotifyingPerson> outer = new ContinuousCollection<NotifyingPerson>();

            var clinqResults = from outerNotifyingPerson in outer
                               join innerNotifyingPerson in inner on outerNotifyingPerson.Age equals innerNotifyingPerson.Age into innersMatchingOuterAge
                               //select new KeyValuePair<NotifyingPerson, ReadOnlyContinuousCollection<NotifyingPerson>>(outerNotifyingPerson, innersMatchingOuterAge);
                               select innersMatchingOuterAge;
            TimeIt(1, () =>
            {

                //List<NotifyingPerson> innerPeople = new List<NotifyingPerson>();
                //for (int i = 0; i < innerItems; i++)
                //{
                //    innerPeople.Add(new NotifyingPerson(i.ToString(), i % outerItems));
                //}

                //inner.AddRange(innerPeople);

                for (int i = 0; i < innerItems; i++)
                {
                    inner.Add(new NotifyingPerson(i.ToString(), i % outerItems));
                }

                for (int i = 0; i < outerItems; i++)
                {
                    outer.Add(new NotifyingPerson(i.ToString(), i));
                }

                //for (int i = 0; i < outerItems; i++)
                //{
                //    outer.Move(rand.Next(outerItems), rand.Next(outerItems));
                //}

                //for (int i = outerItems - 1; i >= 0; i--)
                //{
                //    //if ((rand.Next() & 1) == 0)
                //    if(i % 2 == 0)
                //    {
                //        outer.RemoveAt(i);
                //    }
                //}

                //for (int i = 0; i < innerItems; i++)
                //{
                //    inner.Move(rand.Next(innerItems), rand.Next(innerItems));
                //}

                //for (int i = innerItems - 1; i >= 0; i--)
                //{
                //    if ((rand.Next() & 1) == 0)
                //    {
                //        inner.RemoveAt(i);
                //    }
                //}
            });

            Console.WriteLine(clinqResults.Count);
        }

        //public void MySkipListVsLomontAdds()
        //{
        //    Console.WriteLine("MySkipListVsLomontAdds");

        //    int[] keys = CreateRandomSetOfKeys(ITEMS);

        //    Console.WriteLine("SkipList");
        //    SkipList<int, int> skipList = new SkipList<int, int>();
        //    TimeIt(TRIALS, () =>
        //    {
        //        for (int i = 0; i < ITEMS; i++)
        //        {
        //            skipList.Add(keys[i], i);
        //        }
        //    });

        //    Console.WriteLine("Lomont");
        //    Lomont.LomontSkipList<int, int> lomontSkipList = new Lomont.LomontSkipList<int, int>();
        //    TimeIt(TRIALS, () =>
        //    {
        //        for (int i = 0; i < ITEMS; i++)
        //        {
        //            lomontSkipList.Add(keys[i], i);
        //        }
        //    });
        //}

        //public void MySkipListVsLomontLookups()
        //{
        //    Console.WriteLine("MySkipListVsLomontLookups");

        //    int[] keys = CreateRandomSetOfKeys(ITEMS);

        //    Console.WriteLine("SkipList");
        //    SkipList<int, int> skipList = new SkipList<int, int>();
        //    for (int i = 0; i < ITEMS; i++)
        //    {
        //        skipList.Add(keys[i], i);
        //    }

        //    TimeIt(TRIALS, () =>
        //    {
        //        int val;
        //        for (int i = 0; i < ITEMS; i++)
        //        {
        //            val = skipList.GetValue(keys[i]);
        //        }
        //    });

        //    Console.WriteLine("Lomont");
        //    Lomont.LomontSkipList<int, int> lomont = new Lomont.LomontSkipList<int, int>();
        //    for (int i = 0; i < ITEMS; i++)
        //    {
        //        lomont.Add(i, i);
        //    }
        //    TimeIt(TRIALS, () =>
        //    {
        //        int val;
        //        for (int i = 0; i < ITEMS; i++)
        //        {
        //            val = lomont[keys[i]];
        //        }
        //    });
        //}


        //public void LlrbtVsSortedDictionaryAdds()
        //{
        //    Console.WriteLine("LlrbtVsSortedDictionaryAdds");

        //    int[] keys = CreateRandomSetOfKeys(ITEMS);
        //    Console.WriteLine("LLRBT");
        //    //Console.ReadLine();
        //    LeftLeaningRedBlackTree<int, int> llrbt = new LeftLeaningRedBlackTree<int, int>(Comparer<int>.Default.Compare);
        //    TimeIt(TRIALS, () =>
        //    {
        //        for (int i = 0; i < ITEMS; i++)
        //        {
        //            llrbt.Add(keys[i], i);
        //        }
        //    });

        //    Console.WriteLine("SortedDictionary");
        //    SortedDictionary<int, int> sortedDictionary = new SortedDictionary<int, int>();
        //    TimeIt(TRIALS, () =>
        //    {
        //        for (int i = 0; i < ITEMS; i++)
        //        {
        //            sortedDictionary.Add(keys[i], i);
        //        }
        //    });
        //}

        //public void LlrbtVsSortedDictionaryLookups()
        //{
        //    Console.WriteLine("LlrbtVsSortedDictionaryLookups");
        //    int[] keys = CreateRandomSetOfKeys(ITEMS);

        //    Console.WriteLine("SortedDictionary");
        //    SortedDictionary<int, int> sortedDictionary = new SortedDictionary<int, int>();
        //    for (int i = 0; i < ITEMS; i++)
        //    {
        //        sortedDictionary.Add(keys[i], i);
        //    }

        //    TimeIt(TRIALS, () =>
        //    {
        //        int val;
        //        for (int i = 0; i < ITEMS; i++)
        //        {
        //            val = sortedDictionary[keys[i]];
        //        }
        //    });

        //    Console.WriteLine("LLRBT");
        //    LeftLeaningRedBlackTree<int, int> llrbt = new LeftLeaningRedBlackTree<int, int>(Comparer<int>.Default.Compare);
        //    for (int i = 0; i < ITEMS; i++)
        //    {
        //        llrbt.Add(keys[i], i);
        //    }

        //    TimeIt(TRIALS, () =>
        //    {
        //        int val;
        //        for (int i = 0; i < ITEMS; i++)
        //        {
        //            val = llrbt.GetValueForKey(keys[i]);
        //        }
        //    });
        //}
    }
}
