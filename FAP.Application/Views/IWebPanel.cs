using System.Waf.Applications;

namespace FAP.Application.Views
{
    public interface IWebPanel : IView
    {
        string Location { set; get; }
    }
}