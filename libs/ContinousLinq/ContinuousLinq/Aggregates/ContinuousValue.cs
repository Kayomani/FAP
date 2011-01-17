using System.ComponentModel;

namespace ContinuousLinq.Aggregates
{
    public class ContinuousValue
    {
    }

    public sealed class ContinuousValue<T> : ContinuousValue, INotifyPropertyChanged
    {
        private T _realValue;

        internal object SourceAdapter
        { get; set; }
            
               
        public T CurrentValue
        {
            get
            {
                return _realValue;
            }
            internal set
            {
                _realValue = value;                               
                if (PropertyChanged != null)
                {                
                   PropertyChanged(this, new PropertyChangedEventArgs("CurrentValue"));
                }
            }
        }

        public override string ToString()
        {
            return _realValue.ToString();
        }

        public void Pause()
        {
            (SourceAdapter as AggregateViewAdapter).Pause();
        }

        public void Resume()
        {
            (SourceAdapter as AggregateViewAdapter).Resume();
        }

        public bool IsPaused
        {
            get
            {
                return (SourceAdapter as AggregateViewAdapter).IsPaused;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
