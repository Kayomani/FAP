using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Fap.Foundation
{
    public class RegistryHelper
    {
        public static string GetRegistryData(RegistryKey key, string path)
        {
            string[] keys = path.Split('/');
            try
            {
                if (keys.Count() == 1)
                {
                    object value = key.GetValue(keys[0]);
                    if (null != value)
                        return value.ToString();
                    return null;
                }
                else
                {
                    //Open subkey
                    RegistryKey sub = key.OpenSubKey(keys[0]);
                    return GetRegistryData(sub, path.Substring(keys[0].Length + 1));
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void SetRegistryData(RegistryKey key, string path, string item, string value)
        {
            string[] keys = path.Split(new char[] {'/'},StringSplitOptions.RemoveEmptyEntries);
            try
            {
                if (keys.Count() == 0)
                {
                    key.SetValue(item, value);
                    key.Close();
                }
                else
                {
                    string[] subKeys = key.GetSubKeyNames();
                    if (subKeys.Where(s => s.ToLowerInvariant() == keys[0].ToLowerInvariant()).FirstOrDefault() != null)
                    {
                        //Open subkey
                        RegistryKey sub = key.OpenSubKey(keys[0], (keys.Count() == 1));
                        if (keys.Length > 1)
                            SetRegistryData(sub, path.Substring(keys[0].Length + 1), item, value);
                        else
                            SetRegistryData(sub, string.Empty, item, value);
                    }
                    else
                    {
                        SetRegistryData(key.CreateSubKey(keys[0]), path.Substring(keys[0].Length + 1), item, value);
                    }
                }
            }
            catch { }
        }
    }
}
