using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Management;
using System.IO;
using System.Reflection;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Represents an WMI context, and is responsible for opening, closing and quering to the WMI 
    /// We are now using the WMICOntext both as an context and as an session (Ie. Context.CreateSession()), this should be 2 seperate
    /// entities later on but for time sake, i'll just put them togheter
    /// </summary>
    public class WmiContext : IDisposable
    {
        private ManagementScope _managementScope = null;

        private WmiQueryBuilder _builder = null;
        private string _host = null;
        private TextWriter _log = null;

        /// <summary>
        /// Creates an new WMIContext with the specified host parameter
        /// </summary>
        /// <param name="host"></param>
        public WmiContext(string host)
        {
            _builder = new WmiQueryBuilder(this);
            _host = host;
        }

        /// <summary>
        /// Make sure the connection exists
        /// </summary>
        private void EnsureConnectionCreated()
        {
            if (_managementScope == null)
            {
                Connect();
            }
        }

        public TextWriter Log
        {
            set
            {
                _log = value;
            }
        }

        /// <summary>
        /// Creates in-fact an new WMI Query object
        /// </summary>
        public WmiQuery<T> Source<T>()
        {
            return new WmiQuery<T>(this);
        }

        /// <summary>
        /// Clean-up
        /// </summary>
        public void Dispose()
        {
            if (_managementScope != null)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Executes an WMI query (which is actually an wrapped expression)
        /// </summary>
        internal IEnumerator ExecuteWmiQuery(IWmiQuery query)
        {
            EnsureConnectionCreated();
            string wmiQueryStatement = _builder.BuildQuery(query);

            WriteLog(wmiQueryStatement);

            ObjectQuery objQuery = new ObjectQuery(wmiQueryStatement);
            ManagementObjectSearcher wmiSearcher = new ManagementObjectSearcher(_managementScope, objQuery);

            //Generate a new generic type
            Type genericType = typeof(WmiObjectEnumerator<>).MakeGenericType(query.Type);
            
            ConstructorInfo genericCollection = genericType.GetConstructor(new Type[] { typeof(ManagementObjectCollection) });

            return (IEnumerator)genericCollection.Invoke(new object[] { wmiSearcher.Get() });
        }

        /// <summary>
        /// Connect and create an new management scope
        /// </summary>
        private void Connect()
        {
            _managementScope = new ManagementScope(_host, new ConnectionOptions());
        }

        /// <summary>
        /// The scope of the WMI query
        /// </summary>
        public ManagementScope ManagementScope
        {
            get
            {
                if (_managementScope == null)
                {
                    Connect();
                }

                return _managementScope;
            }
        }

        /// <summary>
        /// Disconnect 
        /// </summary>
        private void Disconnect()
        {
        }

        /// <summary>
        /// Helper function to write to a log
        /// </summary>
        /// <param name="txt"></param>
        private void WriteLog(string txt)
        {
            if (_log != null)
            {
                _log.WriteLine(String.Format("LOG: {0}", txt));
            }
        }
    }
}
