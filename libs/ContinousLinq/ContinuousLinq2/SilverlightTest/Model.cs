using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ContinuousLinq.Reactive;

namespace SilverlightTest
{
    public class Model : ReactiveObject
    {
        private int _intProperty;
        public int IntProperty
        {
            get { return _intProperty; }
            set
            {
                if (value == _intProperty)
                    return;

                OnPropertyChanging("IntProperty");
                _intProperty = value;
                OnPropertyChanged("IntProperty");
            }
        }

        private Model _childModel;
        public Model ChildModel
        {
            get { return _childModel; }
            set
            {
                if (value == _childModel)
                    return;

                OnPropertyChanging("ChildModel");
                _childModel = value;
                OnPropertyChanged("ChildModel");
            }
        }
        
        static Model()
        {
            var dependsOn = Register<Model>();
            
            dependsOn.Call(obj => obj.OnIntPropertyChanged())
                .OnChanged(obj => obj.IntProperty);
            
            dependsOn.Call(obj => obj.OnChildModelIntPropertyChanged())
                .OnChanged(obj => obj.ChildModel.IntProperty);
        }

        private void OnIntPropertyChanged()
        {
            int a = 0;
            a++;
        }

        private void OnChildModelIntPropertyChanged()
        {
            int a = 0;
            a++;
        }
    }
}
