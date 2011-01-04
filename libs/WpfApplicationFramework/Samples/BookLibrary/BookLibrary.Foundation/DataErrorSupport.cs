using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BookLibrary.Foundation
{
    public delegate string ValidationRuleDelegate(object instance, string memberName);


    public class DataErrorSupport : IDataErrorInfo
    {
        private readonly Dictionary<string, ValidationRuleDelegate> validationRules;
        private readonly List<ValidationRuleDelegate> validationRuleList;
        private readonly object instance;


        public DataErrorSupport(object instance)
        {
            if (instance == null) { throw new ArgumentNullException("instance"); }
            this.instance = instance;

            this.validationRules = new Dictionary<string, ValidationRuleDelegate>();
            this.validationRuleList = new List<ValidationRuleDelegate>();
        }


        public string Error
        {
            get { return this[""]; }
        }

        public string this[string memberName]
        {
            get
            {
                memberName = memberName ?? "";
                string errorMessage = "";
                if (string.IsNullOrEmpty(memberName))
                {
                    // We need to use a List because the Dictionary.ValuesCollection doesn't preserve the order.
                    errorMessage = ExecuteValidationRules(validationRuleList, memberName);
                }
                else
                {
                    List<ValidationRuleDelegate> rules = new List<ValidationRuleDelegate>();

                    if (validationRules.ContainsKey(memberName))
                    {
                        rules.Add(validationRules[memberName]);
                    }
                    if (validationRules.ContainsKey("")) 
                    {
                        // The default validation rule is always executed.
                        rules.Add(validationRules[""]);
                    }
                    
                    errorMessage = ExecuteValidationRules(rules, memberName);
                }
                return errorMessage;
            }
        }


        public DataErrorSupport AddValidationRule(string memberName, ValidationRuleDelegate validationRule)
        {
            memberName = memberName ?? "";
            if (validationRules.ContainsKey(memberName))
            {
                throw new ArgumentException("A ValidationRule with the same memberName '"
                    + memberName + "' is already registered.");
            }

            validationRules.Add(memberName, validationRule);
            validationRuleList.Add(validationRule);
            return this;
        }

        private string ExecuteValidationRules(IEnumerable<ValidationRuleDelegate> validationRules, string memberName)
        {
            StringBuilder errorBuilder = new StringBuilder();
            foreach (ValidationRuleDelegate validationRule in validationRules)
            {
                string error = validationRule(instance, memberName);
                if (!string.IsNullOrEmpty(error))
                {
                    errorBuilder.AppendInNewLine(error);
                }
            }
            return errorBuilder.ToString();
        }
    }
}
