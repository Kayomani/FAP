using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightTest
{
    public partial class MainPage : UserControl
    {
        public Model Model {get; set;}

       
        public MainPage()
        {
            InitializeComponent();

            this.Model = new Model();
            this.Model.IntProperty = 100;
            this.Model.ChildModel = new Model();
            this.Model.ChildModel.IntProperty = 10;

            DataContext = Model;
        }
    }
}
