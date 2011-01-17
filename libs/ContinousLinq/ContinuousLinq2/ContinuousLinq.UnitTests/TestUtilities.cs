using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    public class TestUtilities
    {
        //public static void AssertCollectionChangedEventArgsEqual(NotifyCollectionChangedEventArgs expectedEventArgs, NotifyCollectionChangedEventArgs actualEventArgs)
        //{
        //    Assert.AreEqual(expectedEventArgs.Action, actualEventArgs.Action);

        //    if (actualEventArgs.NewItems == null)
        //        Assert.IsNull(expectedEventArgs.NewItems);
        //    else
        //        CollectionAssert.AreEquivalent(expectedEventArgs.NewItems, actualEventArgs.NewItems);

        //    Assert.AreEqual(expectedEventArgs.NewStartingIndex, actualEventArgs.NewStartingIndex);

        //    if (actualEventArgs.OldItems == null)
        //        Assert.IsNull(expectedEventArgs.OldItems);
        //    else
        //        CollectionAssert.AreEquivalent(expectedEventArgs.OldItems, actualEventArgs.OldItems);

        //    Assert.AreEqual(expectedEventArgs.OldStartingIndex, actualEventArgs.OldStartingIndex);
        //}


        public static void AssertAdd(NotifyCollectionChangedEventArgs actualEventArgs, int startingIndex, params object[] newItems)
        {
            Assert.AreEqual(NotifyCollectionChangedAction.Add, actualEventArgs.Action);

            if (actualEventArgs.NewItems == null)
                Assert.IsNull(newItems);
            else
                CollectionAssert.AreEquivalent(newItems, actualEventArgs.NewItems);

            Assert.AreEqual(startingIndex, actualEventArgs.NewStartingIndex);

            Assert.IsNull(actualEventArgs.OldItems);
        }

        public static void AssertRemove(NotifyCollectionChangedEventArgs actualEventArgs, int startingIndex, params object[] oldItems)
        {
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, actualEventArgs.Action);

            Assert.IsNull(actualEventArgs.NewItems);
            
            CollectionAssert.AreEquivalent(oldItems, actualEventArgs.OldItems);

            Assert.AreEqual(startingIndex, actualEventArgs.OldStartingIndex);
        }

        public static void AssertReplace(NotifyCollectionChangedEventArgs actualEventArgs, int startingIndex, object[] newItems, object[] oldItems)
        {   
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, actualEventArgs.Action);

            CollectionAssert.AreEquivalent(newItems, actualEventArgs.NewItems);

            Assert.AreEqual(startingIndex, actualEventArgs.NewStartingIndex);

            CollectionAssert.AreEquivalent(oldItems, actualEventArgs.OldItems);

#if !SILVERLIGHT
            Assert.AreEqual(startingIndex, actualEventArgs.OldStartingIndex);
#endif
        }

        public static void AssertReset(NotifyCollectionChangedEventArgs actualEventArgs)
        {
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, actualEventArgs.Action);
        }

        public static List<NotifyCollectionChangedEventArgs> GetCollectionChangedEventArgsList(INotifyCollectionChanged target)
        {
            var eventArgsList = new List<NotifyCollectionChangedEventArgs>();
            target.CollectionChanged += (sender, e) => eventArgsList.Add(e);
            return eventArgsList;
        }
    }

    public class TestContinuousCollection<T> : ContinuousCollection<T>
    {
        public TestContinuousCollection()
        {
        }

        public TestContinuousCollection(List<T> list)
            : base(list)
        {
        }

        public void FireReset()
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}