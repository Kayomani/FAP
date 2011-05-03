using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Components.Common.TemplateEngine.NVelocityTemplateEngine;
using Castle.Components.Common.TemplateEngine;
using System.IO;
using System.ComponentModel;



namespace FAP.Domain.Services
{
    public class DisplayInfo
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();

        public void SetData(string name, object o)
        {
            if (data.ContainsKey(name))
                data[name] = o;
            data.Add(name, o);
        }

        public object GetData(string name)
        {
            if (data.ContainsKey(name))
                return data[name];
            return string.Empty;
        }

        public void Clear()
        {
            data.Clear();
        }

    }

    public class TemplateEngineService
    {
        private static NVelocityTemplateEngine engine = null;
        private static object sync = new object();

        public static string Generate(string input, Dictionary<string, object> data)
        {
            //Start engine and cache 
            if (null == engine)
            {
                lock (sync)
                {
                    if (null == engine)
                    {
                        NVelocityTemplateEngine e = new NVelocityTemplateEngine();

                        (e as ISupportInitialize).BeginInit();
                        engine = e;
                    }
                }
            }
            using (var writer = new StringWriter())
            {
                engine.Process(data, string.Empty, writer, input);
                return writer.ToString();
            }
        }
    }
}
