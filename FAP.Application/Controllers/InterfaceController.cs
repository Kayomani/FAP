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

using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using FAP.Application.ViewModels;
using FAP.Domain.Entities;

namespace FAP.Application.Controllers
{
    public class InterfaceController
    {
        private readonly InterfaceSelectionViewModel vm;
        private IMessageService message;
        private bool quit;

        public InterfaceController(InterfaceSelectionViewModel v, IMessageService m)
        {
            vm = v;
            message = m;
            vm.Interfaces = new BindingList<NetInterface>();

            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && nic.Supports(NetworkInterfaceComponent.IPv4))
                {
                    var i = new NetInterface {Description = nic.Description, Name = nic.Name, Speed = nic.Speed};
                    IPInterfaceProperties ipProps = nic.GetIPProperties();

                    foreach (UnicastIPAddressInformation address in ipProps.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork &&
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
            foreach (IPAddress t in localIPs)
            {
                vm.SelectedInterface = vm.Interfaces.Where(s => Equals(s.Address, t)).FirstOrDefault();
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
            if (!vm.Interfaces.Any(t => string.Equals(t.Address.ToString(), a)))
            {
                if (vm.Interfaces.Count == 1)
                    return vm.Interfaces[0].Address.ToString();
                vm.ShowDialog();
                if (quit)
                    return null;
                return vm.SelectedInterface.Address.ToString();
            }

            return a;
        }
    }
}