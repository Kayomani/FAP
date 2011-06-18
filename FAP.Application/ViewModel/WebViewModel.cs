using System.Waf.Applications;
using FAP.Application.Views;

namespace FAP.Application.ViewModel
{
    public class WebViewModel : ViewModel<IWebPanel>
    {
        public WebViewModel(IWebPanel view)
            : base(view)
        {
        }

        public string Location
        {
            set { ViewCore.Location = value; }
            get { return ViewCore.Location; }
        }
    }
}