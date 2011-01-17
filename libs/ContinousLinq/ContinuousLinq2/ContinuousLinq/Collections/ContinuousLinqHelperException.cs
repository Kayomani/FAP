using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using ContinuousLinq.Expressions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ContinuousLinq
{
    public class ContinuousLinqHelperException : Exception
    {
        const string MESSAGE = "This exception's stack trace shows where the Continuous Linq query was declared.  Check the inner exception for actual error";

        public ContinuousLinqHelperException(StackTrace stackTraceOfCollectionDeclaration, Exception actualException)
            : base(MESSAGE, actualException)
        {
            this.StackTraceOfCollectionDeclaration = stackTraceOfCollectionDeclaration;
        }

        public StackTrace StackTraceOfCollectionDeclaration { get; private set; }

        public override string StackTrace
        {
            get
            {
                string stackTrace = this.StackTraceOfCollectionDeclaration.ToString();

                stackTrace = Regex.Replace(stackTrace, @"at\s+ContinuousLinq\..*?\n", string.Empty);
                return stackTrace;
            }
        }
    }
}
