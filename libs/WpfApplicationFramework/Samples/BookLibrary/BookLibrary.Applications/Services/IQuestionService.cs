using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookLibrary.Applications.Services
{
    public interface IQuestionService
    {
        bool? ShowQuestion(string message);

        bool ShowYesNoQuestion(string message);
    }
}
