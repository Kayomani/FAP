using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;



namespace FAP.Domain.Services
{
    public class TemplateEngineService
    {
        private static object sync = new object();

        public static string Generate(string input, Dictionary<string, object> data)
        {
            Antlr3.ST.StringTemplate template = new Antlr3.ST.StringTemplate(input);
           
            foreach (var d in data)
                template.SetAttribute(d.Key, d.Value);

            string result =template.ToString();;
            template.Reset();
            data.Clear();
            template = null;
            return result;
            
        }
    }
}
