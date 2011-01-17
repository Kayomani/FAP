using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ContinuousLinq
{
    internal sealed class ExceptViewAdapter<T> : IViewAdapter, IWeakEventListener       
    {
        #region Constructor

        public ExceptViewAdapter(InputCollectionWrapper<T> input1, InputCollectionWrapper<T> input2, LinqContinuousCollection<T> output)
        {
            if (input1 == null)
                throw new ArgumentNullException("input1");
            if (input2 == null)
                throw new ArgumentNullException("input2");
            if (output == null)
                throw new ArgumentNullException("output");

            this.Input1 = input1;
            this.Input2 = input2;
            this.Output = new OutputCollectionWrapper<T>(output);

            // Backreference
            output.SourceAdapter = this;

            // Subscribe to collection / property change
            CollectionChangedEventManager.AddListener(input1.InnerAsNotifier, this);
            CollectionChangedEventManager.AddListener(input2.InnerAsNotifier, this);
           
            // Evaluate changes
            ReEvaluate();
        }

        #endregion

        #region Properties

        private InputCollectionWrapper<T> Input1 { get; set; }
        
        private InputCollectionWrapper<T> Input2 { get; set; }

        private OutputCollectionWrapper<T> Output { get; set; }

        public IViewAdapter PreviousAdapter
        {
            get { return this.Input1.SourceAdapter; }
        }

        #endregion

        #region Methods

        public void ReEvaluate()
        {
            this.Output.Clear();
            this.Output.AddRange(this.Input1.InnerAsList.Except(this.Input2.InnerAsList));    
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(CollectionChangedEventManager))
                return false;

            OnCollectionChanged(sender, (NotifyCollectionChangedEventArgs)e);
            return true;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:                                 
                case NotifyCollectionChangedAction.Remove:
                    ReEvaluate();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.Output.Clear();                    
                    break;
            }
        }

        #endregion
    }
}
