#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Fap.Foundation.Logging
{
    public class Logger: AsyncControllerBase, INotifyCollectionChanged
    {
        private SafeObservable<Log> collection = new SafeObservable<Log>();
        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        public SafeObservable<Log> Logs
        {
            get { return collection; }
        }

        public Logger()
        {
            collection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(collection_CollectionChanged);
        }

        void collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (null != CollectionChanged)
                CollectionChanged(sender, e);
        }


        public void AddWarning(string message)
        {
            //Do it async to stop deadlocks..
            QueueWork(new System.Waf.Applications.DelegateCommand(AddWarningAsync), message);
        }

        private void AddWarningAsync(object message)
        {
            Log l = new Log();
            l.Message = (string)message;
            l.Type = Log.LogType.Warning;
            l.When = DateTime.Now;
            collection.Add(l);
        }

        public void AddInfo(string message)
        {
            //Do it async to stop deadlocks..
            QueueWork(new System.Waf.Applications.DelegateCommand(AddInfoAsync), message);
        }

        private void AddInfoAsync(object message)
        {
            Log l = new Log();
            l.Message = (string)message;
            l.Type = Log.LogType.Info;
            l.When = DateTime.Now;
            collection.Add(l);
        }


        public void AddError(string message)
        {

            //Do it async to stop deadlocks..
            QueueWork(new System.Waf.Applications.DelegateCommand(AddErrorAsync), message);
        }


        private void AddErrorAsync(object message)
        {
            Log l = new Log();
            l.Message = (string)message;
            l.Type = Log.LogType.Error;
            l.When = DateTime.Now;
            collection.Add(l);
        }

        public void LogException(Exception e)
        {
            string msg = "Exception " + e.Message;
            if (e.InnerException != null)
                msg += " Inner Exception: " + e.InnerException.Message;
            AddErrorAsync(msg);
        }
    }
}
