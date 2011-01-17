using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using ContinuousLinq.Collections;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    public abstract class BaseReadOnlyCollectionTest<TargetType, TResult>
        where TargetType : ReadOnlyAdapterContinuousCollection<Person, TResult>
    {
        protected ContinuousCollection<Person> _source;
        protected Func<IEnumerator<AssertVariables>, Func<NotifyCollectionChangedEventHandler>, NotifyCollectionChangedEventHandler> _collectionChangedHandler;
        protected bool collectionChangedHandlersCompleted;
        protected TargetType _target;
        protected struct AssertVariables
        {
            public NotifyCollectionChangedAction Action;
            public int OldIndex;
            public int NewIndex;
            public Person[] OldPersons;
            public Person[] NewPersons;
        }
        protected abstract Func<TargetType> TargetFactory
        { 
            get;
        }
        protected abstract void AssertTargetMatchesSource();
        private void setupTargetAndHandlers()
        {
            _target = TargetFactory();
            registerCollectionChangedHandlerShouldntBeRaised();
            setupCollectionChangedHandler();
        }
        protected void SetUp6PersonSource()
        {
            _source = new ContinuousCollection<Person>(ClinqTestFactory.CreateSixPersonSource().ToList());
            setupTargetAndHandlers();
        }
        protected void SetUp2PersonSource()
        {
            _source = new ContinuousCollection<Person>(ClinqTestFactory.CreateTwoPersonSource().ToList());
            setupTargetAndHandlers();
        }
        protected  void SetUp10PersonSource()
        {
            _source = new ContinuousCollection<Person>(ClinqTestFactory.CreateAnyPersonSource(10).ToList());
            setupTargetAndHandlers();
        }
        private void setupCollectionChangedHandler()
        {
            _collectionChangedHandler = (assertVars, getNotifyCollectionChanged) => (sender, args) =>
            {
                Assert.AreEqual(assertVars.Current.Action, args.Action);
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        Assert.AreEqual(assertVars.Current.OldIndex, args.OldStartingIndex);
                        Assert.AreEqual(assertVars.Current.OldPersons, args.OldItems.ToArray<Person>());
                        break;
                    case NotifyCollectionChangedAction.Add:
                        Assert.AreEqual(assertVars.Current.NewIndex, args.NewStartingIndex);
                        Assert.AreEqual(assertVars.Current.NewPersons, args.NewItems.ToArray<Person>());
                        break;
#if !SILVERLIGHT
                    case NotifyCollectionChangedAction.Move:
                        Assert.AreEqual(assertVars.Current.NewIndex, args.NewStartingIndex);
                        Assert.AreEqual(assertVars.Current.OldIndex, args.OldStartingIndex);
                        Assert.AreEqual(assertVars.Current.NewPersons, args.NewItems.ToArray<Person>());
                        Assert.AreEqual(assertVars.Current.OldPersons, args.OldItems.ToArray<Person>());
                        break;
#endif
                    case NotifyCollectionChangedAction.Replace:
                        Assert.AreEqual(assertVars.Current.NewIndex, args.NewStartingIndex);
                        Assert.AreEqual(assertVars.Current.NewPersons, args.NewItems.ToArray<Person>());
                        Assert.AreEqual(assertVars.Current.OldPersons, args.OldItems.ToArray<Person>());
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        Assert.IsNull(args.OldItems);
                        Assert.IsNull(args.NewItems);
                        break;
                }
                AssertTargetMatchesSource();
                _target.CollectionChanged -= getNotifyCollectionChanged();
                registerCollectionChangedHandlerShouldntBeRaised();
                registerNextCollectionChangedHandler(assertVars);
            };
        }
        protected void registerCollectionChangedAssertions(params AssertVariables[] assertVariables)
        {
            registerNextCollectionChangedHandler(assertVariables.GetEnumerator<AssertVariables>());
        }
        private void registerNextCollectionChangedHandler(IEnumerator<AssertVariables> assertVariablesEnumerator)
        {
            collectionChangedHandlersCompleted = false;
            if (!assertVariablesEnumerator.MoveNext())
            {
                collectionChangedHandlersCompleted = true;
                return;
            }
            _target.CollectionChanged -= shouldntBeRaisedCollectionChangedHandler;
            NotifyCollectionChangedEventHandler notifyCollectionChanged = null;
            Func<NotifyCollectionChangedEventHandler> getNotifyCollectionChanged = () => notifyCollectionChanged;
            _target.CollectionChanged += notifyCollectionChanged = _collectionChangedHandler(assertVariablesEnumerator, getNotifyCollectionChanged);
        }

        
        private AssertVariables forNextActionIndexAndPersons(NotifyCollectionChangedAction action, int newindex, int oldindex, Person[] newPeople, Person[] oldPeople)
        {
            return new AssertVariables { Action = action, OldIndex = oldindex, NewIndex = newindex, OldPersons = oldPeople, NewPersons = newPeople };
        }
        protected AssertVariables forAddWithNewIndexAndAddedPersons(int newIndex, params Person[] people)
        {
            return forNextActionIndexAndPersons(NotifyCollectionChangedAction.Add, newIndex, -1, people, null);
        }
        protected AssertVariables forRemoveWithOldIndexAndRemovedPersons(int oldIndex, params Person[] people)
        {
            return forNextActionIndexAndPersons(NotifyCollectionChangedAction.Remove, -1, oldIndex, null, people);
        }
        protected AssertVariables forReplaceWithIndexNewPersonsAndReplacedPersons(int index, Person[] newpeople, params Person[] replacedPeople)
        {
            return forNextActionIndexAndPersons(NotifyCollectionChangedAction.Replace, index, index, newpeople,
                                                replacedPeople);
        }
#if !SILVERLIGHT
        protected AssertVariables forMoveWithNewIndexOldIndexAndMovedPersons(int newIndex, int oldIndex, params Person[] people)
        {
            return forNextActionIndexAndPersons(NotifyCollectionChangedAction.Move, newIndex, oldIndex, people, people);
        }
#endif
        protected  AssertVariables forReset()
        {
            return forNextActionIndexAndPersons(NotifyCollectionChangedAction.Reset, -1, -1, null, null);
        }
        private void registerCollectionChangedHandlerShouldntBeRaised()
        {
            collectionChangedHandlersCompleted = false;
            _target.CollectionChanged += shouldntBeRaisedCollectionChangedHandler;
        }
        private void shouldntBeRaisedCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            Assert.Fail("CollectionChangedHandler raised in error");
        }
    }
}
