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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using Autofac;
using FAP.Application.ViewModels;
using FAP.Domain.Entities;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace FAP.Application.Controllers
{
    internal class SettingsController
    {
        private readonly IContainer container;
        private readonly ApplicationCore core;
        private readonly DokanController dokan;
        private readonly Model model;
        private QueryViewModel browser;
        private SettingsViewModel viewModel;

        public SettingsController(IContainer c, Model m, ApplicationCore ac)
        {
            container = c;
            model = m;
            core = ac;
            dokan = c.Resolve<DokanController>();
        }

        public SettingsViewModel ViewModel
        {
            get { return viewModel; }
        }

        public void Initaize()
        {
            if (null == viewModel)
            {
                browser = container.Resolve<QueryViewModel>();
                viewModel = container.Resolve<SettingsViewModel>();
                viewModel.Model = model;
                viewModel.EditDownloadDir = new DelegateCommand(SettingsEditDownloadDir);
                viewModel.ChangeAvatar = new DelegateCommand(ChangeAvatar);
                viewModel.ResetInterface = new DelegateCommand(ResetInterface);
                viewModel.DisplayQuickStart = new DelegateCommand(DisplayQuickStart);
                viewModel.AvailibleDrives = new System.Collections.Generic.List<string>();
                viewModel.DokanAction = new DelegateCommand(DokanAction);
                viewModel.DokanDriveLetter = dokan.DriveLetter.ToString();

                //Must use pinvoke to be able to find unconnected network drives
                var currentDrives = new List<string>();
                //Network drives
                foreach(var drive in WNetResource())
                    currentDrives.Add(drive.Key + "\\");
               //Local drives
                currentDrives.AddRange((from a in DriveInfo.GetDrives()
                                  select a.RootDirectory.Name));

                for (int i = Convert.ToInt16('A'); i < Convert.ToInt16('Z'); i++)
                {
                    string drive = new string(new char[] { (char)i });
                    if (!currentDrives.Any(x => string.Equals(drive + ":\\", x, StringComparison.InvariantCultureIgnoreCase)))
                        viewModel.AvailibleDrives.Add(drive);
                }
                //Add currently used item
                if (dokan.Active)
                    viewModel.AvailibleDrives.Add(dokan.DriveLetter.ToString());
                UpdateDokanAction();
                viewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(viewModel_PropertyChanged);
                dokan.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(dokan_PropertyChanged);
            }
        }

        void dokan_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Active")
                UpdateDokanAction();
        }

        void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DokanDriveLetter")
            {
                model.DokanDriveLetter = viewModel.DokanDriveLetter[0];
                dokan.DriveLetter = model.DokanDriveLetter;
            }
        }

        private void UpdateDokanAction()
        {
            if (dokan.IsDokanInstalled)
            {
                if (dokan.Active)
                    viewModel.DokanActionText = "Stop";
                else
                    viewModel.DokanActionText = "Start";
            }
            else
            {
                viewModel.DokanActionText = "Install";
            }
        }

        private void DokanAction()
        {
            if (dokan.IsDokanInstalled)
            {
                if (dokan.Active)
                    dokan.Stop();
                else
                    dokan.Start();
            }
            else
            {
                dokan.InstallDokan();
            }
            UpdateDokanAction();
        }

        private void DisplayQuickStart()
        {
            core.ShowQuickStart();
        }

        private void SettingsEditDownloadDir()
        {
            string folder = string.Empty;
            if (browser.SelectFolder(out folder))
            {
                model.DownloadFolder = folder;
                model.IncompleteFolder = folder + "\\Incomplete";
            }
        }

        private void ChangeAvatar()
        {
            string path = string.Empty;

            if (browser.SelectFile(out path))
            {
                try
                {
                    var ms = new MemoryStream();
                    var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    ms.SetLength(stream.Length);
                    stream.Read(ms.GetBuffer(), 0, (int) stream.Length);
                    ms.Flush();
                    stream.Close();
                    //Resize
                    var bitmap = new Bitmap(ms);
                    Image thumbnail = ResizeImage(bitmap, 100, 100);
                    ms = new MemoryStream();
                    thumbnail.Save(ms, ImageFormat.Png);
                    model.Avatar = Convert.ToBase64String(ms.ToArray());
                }
                catch
                {
                }
            }
        }

        private Image ResizeImage(Bitmap FullsizeImage, int NewWidth, int MaxHeight)
        {
            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);

            if (FullsizeImage.Width <= NewWidth)
                NewWidth = FullsizeImage.Width;

            int NewHeight = FullsizeImage.Height*NewWidth/FullsizeImage.Width;
            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width*MaxHeight/FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);
            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();
            // Save resized picture
            return NewImage;
        }

        private void ResetInterface()
        {
            model.LocalNode.Host = null;
            model.Save();
            container.Resolve<IMessageService>().ShowWarning("Interface selection reset.  FAP will now restart.");
            var notePad = new Process();

            notePad.StartInfo.FileName = Assembly.GetEntryAssembly().CodeBase;
            notePad.StartInfo.Arguments = "WAIT";
            notePad.Start();
            core.Exit();
        }


        [DllImport("MPR.dll", CharSet = CharSet.Auto)]
        static extern int WNetEnumResource(IntPtr hEnum, ref int lpcCount, IntPtr lpBuffer, ref int lpBufferSize);

        [DllImport("MPR.dll", CharSet = CharSet.Auto)]
        static extern int WNetOpenEnum(RESOURCE_SCOPE dwScope, RESOURCE_TYPE dwType, RESOURCE_USAGE dwUsage,
            [MarshalAs(UnmanagedType.AsAny)][In] object lpNetResource, out IntPtr lphEnum);

        [DllImport("MPR.dll", CharSet = CharSet.Auto)]
        static extern int WNetCloseEnum(IntPtr hEnum);

        public enum RESOURCE_SCOPE : uint
        {
            RESOURCE_CONNECTED = 0x00000001,
            RESOURCE_GLOBALNET = 0x00000002,
            RESOURCE_REMEMBERED = 0x00000003,
            RESOURCE_RECENT = 0x00000004,
            RESOURCE_CONTEXT = 0x00000005
        }
        public enum RESOURCE_TYPE : uint
        {
            RESOURCETYPE_ANY = 0x00000000,
            RESOURCETYPE_DISK = 0x00000001,
            RESOURCETYPE_PRINT = 0x00000002,
            RESOURCETYPE_RESERVED = 0x00000008,
        }
        public enum RESOURCE_USAGE : uint
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010,
            RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
        }
        public enum RESOURCE_DISPLAYTYPE : uint
        {
            RESOURCEDISPLAYTYPE_GENERIC = 0x00000000,
            RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001,
            RESOURCEDISPLAYTYPE_SERVER = 0x00000002,
            RESOURCEDISPLAYTYPE_SHARE = 0x00000003,
            RESOURCEDISPLAYTYPE_FILE = 0x00000004,
            RESOURCEDISPLAYTYPE_GROUP = 0x00000005,
            RESOURCEDISPLAYTYPE_NETWORK = 0x00000006,
            RESOURCEDISPLAYTYPE_ROOT = 0x00000007,
            RESOURCEDISPLAYTYPE_SHAREADMIN = 0x00000008,
            RESOURCEDISPLAYTYPE_DIRECTORY = 0x00000009,
            RESOURCEDISPLAYTYPE_TREE = 0x0000000A,
            RESOURCEDISPLAYTYPE_NDSCONTAINER = 0x0000000B
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NetResource
        {
            public RESOURCE_SCOPE dwScope;
            public RESOURCE_TYPE dwType;
            public RESOURCE_DISPLAYTYPE dwDisplayType;
            public RESOURCE_USAGE dwUsage;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string lpLocalName;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string lpRemoteName;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string lpComment;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string lpProvider;
        }

        static System.Collections.Generic.Dictionary<string, NetResource> WNetResource(object resource)
        {
            System.Collections.Generic.Dictionary<string, NetResource> result = new System.Collections.Generic.Dictionary<string, NetResource>();

            int iRet;
            IntPtr ptrHandle = new IntPtr();
            try
            {
                iRet = WNetOpenEnum(
                    RESOURCE_SCOPE.RESOURCE_REMEMBERED, RESOURCE_TYPE.RESOURCETYPE_DISK, RESOURCE_USAGE.RESOURCEUSAGE_ALL,
                    resource, out ptrHandle);
                if (iRet != 0)
                    return null;

                int entries = -1;
                int buffer = 16384;
                IntPtr ptrBuffer = Marshal.AllocHGlobal(buffer);
                NetResource nr;

                iRet = WNetEnumResource(ptrHandle, ref entries, ptrBuffer, ref buffer);
                while ((iRet == 0) || (entries > 0))
                {
                    Int32 ptr = ptrBuffer.ToInt32();
                    for (int i = 0; i < entries; i++)
                    {
                        nr = (NetResource)Marshal.PtrToStructure(new IntPtr(ptr), typeof(NetResource));
                        if (RESOURCE_USAGE.RESOURCEUSAGE_CONTAINER == (nr.dwUsage
                            & RESOURCE_USAGE.RESOURCEUSAGE_CONTAINER))
                        {
                            //call recursively to get all entries in a container
                            WNetResource(nr);
                        }
                        ptr += Marshal.SizeOf(nr);
                        result.Add(nr.lpLocalName, nr);
                    }

                    entries = -1;
                    buffer = 16384;
                    iRet = WNetEnumResource(ptrHandle, ref entries, ptrBuffer, ref buffer);
                }

                Marshal.FreeHGlobal(ptrBuffer);
                iRet = WNetCloseEnum(ptrHandle);
            }
            catch (Exception)
            {
            }

            return result;
        }
        public static System.Collections.Generic.Dictionary<string, NetResource> WNetResource()
        {
            return WNetResource(null);
        }

    }
}