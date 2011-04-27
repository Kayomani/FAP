namespace WmiEntities
{
    using System;
    using System.Linq;
    using LinqToWmi.Core.WMI;


    public class Win32_LogicalDisk
    {

        private ushort _access;

        private ushort _availability;

        private ulong _blocksize;

        private string _caption;

        private bool _compressed;

        private uint _configmanagererrorcode;

        private bool _configmanageruserconfig;

        private string _creationclassname;

        private string _description;

        private string _deviceid;

        private uint _drivetype;

        private bool _errorcleared;

        private string _errordescription;

        private string _errormethodology;

        private string _filesystem;

        private ulong _freespace;

        private System.DateTime _installdate;

        private uint _lasterrorcode;

        private uint _maximumcomponentlength;

        private uint _mediatype;

        private string _name;

        private ulong _numberofblocks;

        private string _pnpdeviceid;

        private ushort[] _powermanagementcapabilities;

        private bool _powermanagementsupported;

        private string _providername;

        private string _purpose;

        private bool _quotasdisabled;

        private bool _quotasincomplete;

        private bool _quotasrebuilding;

        private ulong _size;

        private string _status;

        private ushort _statusinfo;

        private bool _supportsdiskquotas;

        private bool _supportsfilebasedcompression;

        private string _systemcreationclassname;

        private string _systemname;

        private bool _volumedirty;

        private string _volumename;

        private string _volumeserialnumber;

        // Represents the property Access
        public virtual ushort Access
        {
            get
            {
                return this._access;
            }
            set
            {
                this._access = value;
            }
        }

        // Represents the property Availability
        public virtual ushort Availability
        {
            get
            {
                return this._availability;
            }
            set
            {
                this._availability = value;
            }
        }

        // Represents the property BlockSize
        public virtual ulong BlockSize
        {
            get
            {
                return this._blocksize;
            }
            set
            {
                this._blocksize = value;
            }
        }

        // Represents the property Caption
        public virtual string Caption
        {
            get
            {
                return this._caption;
            }
            set
            {
                this._caption = value;
            }
        }

        // Represents the property Compressed
        public virtual bool Compressed
        {
            get
            {
                return this._compressed;
            }
            set
            {
                this._compressed = value;
            }
        }

        // Represents the property ConfigManagerErrorCode
        public virtual uint ConfigManagerErrorCode
        {
            get
            {
                return this._configmanagererrorcode;
            }
            set
            {
                this._configmanagererrorcode = value;
            }
        }

        // Represents the property ConfigManagerUserConfig
        public virtual bool ConfigManagerUserConfig
        {
            get
            {
                return this._configmanageruserconfig;
            }
            set
            {
                this._configmanageruserconfig = value;
            }
        }

        // Represents the property CreationClassName
        public virtual string CreationClassName
        {
            get
            {
                return this._creationclassname;
            }
            set
            {
                this._creationclassname = value;
            }
        }

        // Represents the property Description
        public virtual string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }

        // Represents the property DeviceID
        public virtual string DeviceID
        {
            get
            {
                return this._deviceid;
            }
            set
            {
                this._deviceid = value;
            }
        }

        // Represents the property DriveType
        public virtual uint DriveType
        {
            get
            {
                return this._drivetype;
            }
            set
            {
                this._drivetype = value;
            }
        }

        // Represents the property ErrorCleared
        public virtual bool ErrorCleared
        {
            get
            {
                return this._errorcleared;
            }
            set
            {
                this._errorcleared = value;
            }
        }

        // Represents the property ErrorDescription
        public virtual string ErrorDescription
        {
            get
            {
                return this._errordescription;
            }
            set
            {
                this._errordescription = value;
            }
        }

        // Represents the property ErrorMethodology
        public virtual string ErrorMethodology
        {
            get
            {
                return this._errormethodology;
            }
            set
            {
                this._errormethodology = value;
            }
        }

        // Represents the property FileSystem
        public virtual string FileSystem
        {
            get
            {
                return this._filesystem;
            }
            set
            {
                this._filesystem = value;
            }
        }

        // Represents the property FreeSpace
        public virtual ulong FreeSpace
        {
            get
            {
                return this._freespace;
            }
            set
            {
                this._freespace = value;
            }
        }

        // Represents the property InstallDate
        public virtual System.DateTime InstallDate
        {
            get
            {
                return this._installdate;
            }
            set
            {
                this._installdate = value;
            }
        }

        // Represents the property LastErrorCode
        public virtual uint LastErrorCode
        {
            get
            {
                return this._lasterrorcode;
            }
            set
            {
                this._lasterrorcode = value;
            }
        }

        // Represents the property MaximumComponentLength
        public virtual uint MaximumComponentLength
        {
            get
            {
                return this._maximumcomponentlength;
            }
            set
            {
                this._maximumcomponentlength = value;
            }
        }

        // Represents the property MediaType
        public virtual uint MediaType
        {
            get
            {
                return this._mediatype;
            }
            set
            {
                this._mediatype = value;
            }
        }

        // Represents the property Name
        public virtual string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        // Represents the property NumberOfBlocks
        public virtual ulong NumberOfBlocks
        {
            get
            {
                return this._numberofblocks;
            }
            set
            {
                this._numberofblocks = value;
            }
        }

        // Represents the property PNPDeviceID
        public virtual string PNPDeviceID
        {
            get
            {
                return this._pnpdeviceid;
            }
            set
            {
                this._pnpdeviceid = value;
            }
        }

        // Represents the property PowerManagementCapabilities
        public virtual ushort[] PowerManagementCapabilities
        {
            get
            {
                return this._powermanagementcapabilities;
            }
            set
            {
                this._powermanagementcapabilities = value;
            }
        }

        // Represents the property PowerManagementSupported
        public virtual bool PowerManagementSupported
        {
            get
            {
                return this._powermanagementsupported;
            }
            set
            {
                this._powermanagementsupported = value;
            }
        }

        // Represents the property ProviderName
        public virtual string ProviderName
        {
            get
            {
                return this._providername;
            }
            set
            {
                this._providername = value;
            }
        }

        // Represents the property Purpose
        public virtual string Purpose
        {
            get
            {
                return this._purpose;
            }
            set
            {
                this._purpose = value;
            }
        }

        // Represents the property QuotasDisabled
        public virtual bool QuotasDisabled
        {
            get
            {
                return this._quotasdisabled;
            }
            set
            {
                this._quotasdisabled = value;
            }
        }

        // Represents the property QuotasIncomplete
        public virtual bool QuotasIncomplete
        {
            get
            {
                return this._quotasincomplete;
            }
            set
            {
                this._quotasincomplete = value;
            }
        }

        // Represents the property QuotasRebuilding
        public virtual bool QuotasRebuilding
        {
            get
            {
                return this._quotasrebuilding;
            }
            set
            {
                this._quotasrebuilding = value;
            }
        }

        // Represents the property Size
        public virtual ulong Size
        {
            get
            {
                return this._size;
            }
            set
            {
                this._size = value;
            }
        }

        // Represents the property Status
        public virtual string Status
        {
            get
            {
                return this._status;
            }
            set
            {
                this._status = value;
            }
        }

        // Represents the property StatusInfo
        public virtual ushort StatusInfo
        {
            get
            {
                return this._statusinfo;
            }
            set
            {
                this._statusinfo = value;
            }
        }

        // Represents the property SupportsDiskQuotas
        public virtual bool SupportsDiskQuotas
        {
            get
            {
                return this._supportsdiskquotas;
            }
            set
            {
                this._supportsdiskquotas = value;
            }
        }

        // Represents the property SupportsFileBasedCompression
        public virtual bool SupportsFileBasedCompression
        {
            get
            {
                return this._supportsfilebasedcompression;
            }
            set
            {
                this._supportsfilebasedcompression = value;
            }
        }

        // Represents the property SystemCreationClassName
        public virtual string SystemCreationClassName
        {
            get
            {
                return this._systemcreationclassname;
            }
            set
            {
                this._systemcreationclassname = value;
            }
        }

        // Represents the property SystemName
        public virtual string SystemName
        {
            get
            {
                return this._systemname;
            }
            set
            {
                this._systemname = value;
            }
        }

        // Represents the property VolumeDirty
        public virtual bool VolumeDirty
        {
            get
            {
                return this._volumedirty;
            }
            set
            {
                this._volumedirty = value;
            }
        }

        // Represents the property VolumeName
        public virtual string VolumeName
        {
            get
            {
                return this._volumename;
            }
            set
            {
                this._volumename = value;
            }
        }

        // Represents the property VolumeSerialNumber
        public virtual string VolumeSerialNumber
        {
            get
            {
                return this._volumeserialnumber;
            }
            set
            {
                this._volumeserialnumber = value;
            }
        }
    }
}
