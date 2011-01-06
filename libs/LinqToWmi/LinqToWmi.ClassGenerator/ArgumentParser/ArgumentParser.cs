using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LinqToWmi.ProtoGenerator
{
    /// <summary>
    /// Simple argument parser
    /// </summary>
    public class ArgumentParser
    {
        /// <summary>
        /// Helper function for retrieving the attribute of our propertyinfo
        /// </summary>
        private static ArgumentAttribute GetAttribute(PropertyInfo info)
        {
            return (ArgumentAttribute)ArgumentAttribute.GetCustomAttribute(info, typeof(ArgumentAttribute));
        }

        /// <summary>
        /// Parses the arguments and returns the argument object, throws an ArgumentParseException
        /// when an error occurs
        /// </summary>
        public static T Create<T>(string[] arguments) where T : new()
        {
            var help = from prop in typeof(T).GetProperties() where GetAttribute(prop) != null
                       select new { Attr = GetAttribute(prop), Prop = prop};
            
            T instance = new T();
            var options = ParseArguments(arguments);

            foreach (var anon in help)
            {
                if (options.ContainsKey("/" + anon.Attr.Cmd))
                {
                    anon.Prop.SetValue(instance, options["/" + anon.Attr.Cmd], null);
                }
                else if (anon.Attr.Required)
                {
                    throw new ArgumentParseException((from a in help select a.Attr).ToArray());
                }
            }
            return instance;
        }

        /// <summary>
        /// Parse the arguments and create a dictionary
        /// </summary>
        private static Dictionary<string, string> ParseArguments(string [] arguments) {            
            
            var options = new Dictionary<string, string>();
            foreach (string argument in arguments)
            {
                string[] cmd = argument.Split(new char[] { ':' }, 2);
                if (cmd.Length == 2)
                {
                    string name = cmd[0].ToLower().Trim();
                    if (!options.ContainsKey(name))
                    {
                        options.Add(name, cmd[1].Trim());
                    }
                }
            }
            return options;
        }
    }
}