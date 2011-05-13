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
using System.ComponentModel;
using System.Waf.Applications;

namespace Fap.Foundation
{
    public abstract class AsyncControllerBase: Controller
    {
        private BackgroundWorker worker = new BackgroundWorker();
        private Queue<AsyncOperation> operations = new Queue<AsyncOperation>();

        public delegate void AsyncControllerJobComplete();
        public event AsyncControllerJobComplete AsyncControllerJobCompleteHandler;

        public int JobCount
        {
            get
            {
                lock (worker)
                {
                    return operations.Count;
                }
            }
        }

        public AsyncControllerBase()
        {
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (null != AsyncControllerJobCompleteHandler)
                AsyncControllerJobCompleteHandler();
            bool hasWork = false;
            lock (worker)
            {
                hasWork = operations.Count > 0;
                if (hasWork && !worker.IsBusy)
                    worker.RunWorkerAsync();
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            AsyncOperation op = null;
            lock (worker)
            {
                if (operations.Count > 0)
                    op = operations.Dequeue();
            }
            if (null != op)
            {
                if (op.Command.CanExecute(op.Object))
                    op.Command.Execute(op.Object);
                e.Result = op;
            }
        }

        protected void QueueWork(DelegateCommand command)
        {
            QueueWork(command, null);
        }

        protected void QueueWork(DelegateCommand command, Object param)
        {
            QueueWork(command, param, null);
        }

        protected void QueueWork(DelegateCommand command, Object param, DelegateCommand completed)
        {
            lock (worker)
            {
                operations.Enqueue(new AsyncOperation() { Command = command, Object = param });
                if (!worker.IsBusy)
                    worker.RunWorkerAsync();
            }
        }
    }
}
