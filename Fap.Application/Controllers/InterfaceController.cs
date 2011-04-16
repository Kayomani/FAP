using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using Fap.Application.ViewModels;
using System.Net;
using System.Waf.Applications;
using System.Windows;
using System.Waf.Applications.Services;
using Fap.Foundation;

namespace Fap.Application.Controllers
{
    public class InterfaceController
    {
        private InterfaceSelectionViewModel vm;
        private bool quit = false;
        private IMessageService message;

        public InterfaceController(InterfaceSelectionViewModel v,IMessageService m)
        {
            vm = v;
            message = m;
            vm.Interfaces = new System.ComponentModel.BindingList<Domain.Entity.NetworkInterface>();

            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && nic.Supports(NetworkInterfaceComponent.IPv4))
                {
                    Domain.Entity.NetworkInterface i = new Domain.Entity.NetworkInterface();
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
                vm.SelectedInterface = vm.Interfaces.Where(s => IPAddress.Equals(s.Address,localIPs[i])).FirstOrDefault();
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
