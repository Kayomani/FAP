using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Services;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Services
{
    [Export(typeof(IQuestionService)), Export]
    public class QuestionServiceMock : IQuestionService
    {
        public Func<string, bool?> ShowQuestionAction { get; set; }
        public Func<string, bool> ShowYesNoQuestionAction { get; set; }


        public bool? ShowQuestion(string message)
        {
            return ShowQuestionAction(message);
        }

        public bool ShowYesNoQuestion(string message)
        {
            return ShowYesNoQuestionAction(message);
        }
    }
}
