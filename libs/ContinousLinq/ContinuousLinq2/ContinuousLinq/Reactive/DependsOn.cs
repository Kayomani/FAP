using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ContinuousLinq.Reactive
{
    internal interface IDependsOn
    {
        void CreateSubscriptions(INotifyPropertyChanged subject, ref List<SubscriptionTree> listToAppendTo);
    }

    public class DependsOn<T> : IDependsOn where T : class, INotifyPropertyChanged
    {
        #region Constructor

        internal DependsOn()
        {
            this.Methods = new List<DependsOnMethod<T>>();
        }

        #endregion

        #region Properties

        private List<DependsOnMethod<T>> Methods { get; set; }

        #endregion

        #region Methods

        public void CreateSubscriptions(INotifyPropertyChanged subject, ref List<SubscriptionTree> listToAppendTo)
        {
            foreach (var dependsOnMethod in this.Methods)
            {
                dependsOnMethod.CreateSubscriptions(subject, ref listToAppendTo);
            }
        }

        public DependsOnMethod<T> Call(Action<T> callback)
        {
            var dependsOnMethod = new DependsOnMethod<T>(callback);
            this.Methods.Add(dependsOnMethod);

            return dependsOnMethod;
        }

        #endregion
    }
}