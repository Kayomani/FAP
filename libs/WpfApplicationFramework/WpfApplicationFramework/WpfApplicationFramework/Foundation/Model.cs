using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Waf.Foundation
{
    /// <summary>
    /// Defines the base class for a model.
    /// </summary>
    [Serializable]
    public class Model : INotifyPropertyChanged
    {
        [NonSerialized]
        private PropertyChangedEventHandler propertyChanged;


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }


        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property name of the property that has changed.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected void RaisePropertyChanged(string propertyName)
        {
            if (WafConfiguration.Debug) { CheckPropertyName(propertyName); }
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (propertyChanged != null) { propertyChanged(this, e); }
        }

        private void CheckPropertyName(string propertyName)
        {
            PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(this)[propertyName];
            if (propertyDescriptor == null)
            {
                throw new InvalidOperationException(string.Format(null,
                    "The property with the propertyName '{0}' doesn't exist.", propertyName));
            }
        }
    }
}
