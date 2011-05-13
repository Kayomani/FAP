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
using System.Text;
using System.Waf.Applications;
using FAP.Application.ViewModels;
using System.Waf.Applications.Services;
using System.Net;
using System.Net.NetworkInformation;
using FAP.Domain.Entities;

namespace FAP.Application.Controllers
{
    public class InterfaceController
    {
        private InterfaceSelectionViewModel vm;
        private bool quit = false;
        private IMessageService message;

        public InterfaceController(InterfaceSelectionViewModel v, IMessageService m)
        {
            vm = v;
            message = m;
            vm.Interfaces = new System.ComponentModel.BindingList<NetInterface>();

            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && nic.Supports(NetworkInterfaceComponent.IPv4))
                {
                    NetInterface i = new NetInterface();
                    i.Description = nic.Description;
                    i.Name = nic.Name;
                    i.Speed = nic.Speed;
                    IPInterfaceProperties ipProps = nic.GetIPProperties();

                    foreach (var address in ipProps.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                            localIPs.Contains(address.Address) &&
                            !IPAddress.IsLoopback(address.Address))
                        {
                            i.Address = address.Address;
                            vm.Interfaces.Add(i);
                            break;
                        }
                    }
                }
            }

            //Select primary interface.  Just go with the lowest
            for (int i = 0; i < localIPs.Length; i++)
            {
                vm.SelectedInterface = vm.Interfaces.Where(s => IPAddress.Equals(s.Address, localIPs[i])).FirstOrDefault();
                if (null != vm.SelectedInterface)
                    break;
            }

            vm.Quit = new DelegateCommand(Quit);
            vm.Select = new DelegateCommand(Select);
        }

        private void Quit()
        {
            quit = true;
            vm.Close();
        }

        private void Select()
        {
            if (null != vm.SelectedInterface)
                vm.Close();
        }

        public string CheckAddress(string a)
        {
            //Check to see if the passed address is still valid, if so just use it
            for (int i = 0; i < vm.Interfaces.Count; i++)
            {
                if (string.Equals(vm.Interfaces[i].Address.ToString(), a))
                    return a;
            }

            if (vm.Interfaces.Count == 1)
                return vm.Interfaces[0].Address.ToString();
            vm.ShowDialog();
            if (quit)
                return null;
            return vm.SelectedInterface.Address.ToString();
        }
    }
}
