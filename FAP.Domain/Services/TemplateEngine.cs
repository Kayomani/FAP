using System.Collections.Generic;
using Antlr3.ST;

namespace FAP.Domain.Services
{
    public class TemplateEngineService
    {
        private static object sync = new object();

        public static string Generate(string input, Dictionary<string, object> data)
        {
            var template = new StringTemplate(input);

            foreach (var d in data)
                template.SetAttribute(d.Key, d.Value);

            string result = template.ToString();
            ;
            template.Reset();
            data.Clear();
            template = null;
            return result;
        }
    }
}