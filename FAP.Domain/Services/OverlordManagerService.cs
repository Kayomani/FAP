#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.

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
using System.Threading;
using Autofac;
using FAP.Domain.Entities;
using FAP.Domain.Net;
using NLog;

namespace FAP.Domain.Services
{
    /// <summary>
    /// A class designed to be called at regular intervals.  It checks to see if there are enough overlords running 
    /// on the LAN or too many and responds as appropriate by launching or killing the local overlord.
    /// </summary>
    public class OverlordManagerService
    {
        private readonly IContainer container;
        private readonly Model model;
        private readonly object sync = new object();
        private ListenerService overlord;
        private LANPeerFinderService peerFinder;

        private bool serverLaunching;

        public OverlordManagerService(IContainer c)
        {
            container = c;
            model = c.Resolve<Model>();
        }

        public bool IsOverlordActive
        {
            get { return overlord != null; }
        }

        public void Start()
        {
            lock (sync)
            {
                if (null == overlord)
                {
                    LogManager.GetLogger("faplog").Info("Launching local overlord.");
                    overlord = new ListenerService(container, true);
                    overlord.Start(40);
                }
            }
        }

        public void Stop()
        {
            lock (sync)
            {
                if (null != overlord)
                {
                    overlord.Stop();
                    overlord = null;
                }
            }
        }


        public void StartAndStopIfNeeded()
        {
            lock (sync)
            {
                if (overlord != null)
                {
                    //TODO: Stop if needed
                    return;
                }
                //Dont allow multiple launchers
                if (serverLaunching)
                    return;
                if (IsNewServerNeeded())
                {
                    //Launch a local overlord
                    serverLaunching = true;
                    ThreadPool.QueueUserWorkItem(LaunchOverlordWithDelay);
                }
            }
        }

        private void LaunchOverlordWithDelay(object o)
        {
            try
            {
                var r = new Random();
                int delay = 0;
                switch (model.OverlordPriority)
                {
                        //case dedicated
                        // 0-1.5 seconds
                    case OverlordPriority.High:
                        delay = r.Next(2000, 3000);
                        break;
                    case OverlordPriority.Normal:
                        delay = r.Next(3000, 5000);
                        break;
                    case OverlordPriority.Low:
                        delay = r.Next(5000, 8000);
                        break;
                }
                LogManager.GetLogger("faplog").Debug("Overlord start delay: {0}", delay);
                Thread.Sleep(delay);
                //If a server is still needed - launch
                if (IsNewServerNeeded())
                    Start();
            }
            finally
            {
                serverLaunching = false;
            }
        }


        private bool IsNewServerNeeded()
        {
            if (null == peerFinder)
                peerFinder = container.Resolve<LANPeerFinderService>();

            List<DetectedNode> localOverlords =
                peerFinder.Peers.ToList().Where(p => (DateTime.Now - p.LastAnnounce).TotalSeconds < 60).ToList();


            int overlords = localOverlords.Count;
            int serversWithFreeSlots = 0;
            int totalUsers = 0;
            int totalSlots = 0;

            foreach (DetectedNode overlord in localOverlords)
            {
                totalUsers += overlord.CurrentUsers;
                totalSlots += overlord.MaxUsers;
                if (overlord.MaxUsers - overlord.CurrentUsers > 5)
                    serversWithFreeSlots++;
            }

            //Rule 1: Atleast one server
            if (serversWithFreeSlots == 0)
                return true;
            else
            {
                //Rule 2: Atleast 1 server per 200 people
                if (totalUsers > serversWithFreeSlots*200)
                    return true;
                else
                {
                    if (0 != totalUsers && 0 != totalSlots)
                    {
                        //Rule 3: Atleast 5% free slots
                        if (((double) totalUsers/totalSlots) > 0.95)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}