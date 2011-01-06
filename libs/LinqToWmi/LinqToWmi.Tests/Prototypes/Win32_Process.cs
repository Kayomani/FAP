namespace WmiEntities {
    using System;
    using System.Linq;
    using LinqToWmi.Core.WMI;
    
    
    public class Win32_Process {
        
        private string _caption;
        
        private string _commandline;
        
        private string _creationclassname;
        
        private System.DateTime _creationdate;
        
        private string _cscreationclassname;
        
        private string _csname;
        
        private string _description;
        
        private string _executablepath;
        
        private ushort _executionstate;
        
        private string _handle;
        
        private uint _handlecount;
        
        private System.DateTime _installdate;
        
        private ulong _kernelmodetime;
        
        private uint _maximumworkingsetsize;
        
        private uint _minimumworkingsetsize;
        
        private string _name;
        
        private string _oscreationclassname;
        
        private string _osname;
        
        private ulong _otheroperationcount;
        
        private ulong _othertransfercount;
        
        private uint _pagefaults;
        
        private uint _pagefileusage;
        
        private uint _parentprocessid;
        
        private uint _peakpagefileusage;
        
        private ulong _peakvirtualsize;
        
        private uint _peakworkingsetsize;
        
        private uint _priority;
        
        private ulong _privatepagecount;
        
        private uint _processid;
        
        private uint _quotanonpagedpoolusage;
        
        private uint _quotapagedpoolusage;
        
        private uint _quotapeaknonpagedpoolusage;
        
        private uint _quotapeakpagedpoolusage;
        
        private ulong _readoperationcount;
        
        private ulong _readtransfercount;
        
        private uint _sessionid;
        
        private string _status;
        
        private System.DateTime _terminationdate;
        
        private uint _threadcount;
        
        private ulong _usermodetime;
        
        private ulong _virtualsize;
        
        private string _windowsversion;
        
        private ulong _workingsetsize;
        
        private ulong _writeoperationcount;
        
        private ulong _writetransfercount;
        
        // Represents the property Caption
        public virtual string Caption {
            get {
                return this._caption;
            }
            set {
                this._caption = value;
            }
        }
        
        // Represents the property CommandLine
        public virtual string CommandLine {
            get {
                return this._commandline;
            }
            set {
                this._commandline = value;
            }
        }
        
        // Represents the property CreationClassName
        public virtual string CreationClassName {
            get {
                return this._creationclassname;
            }
            set {
                this._creationclassname = value;
            }
        }
        
        // Represents the property CreationDate
        public virtual System.DateTime CreationDate {
            get {
                return this._creationdate;
            }
            set {
                this._creationdate = value;
            }
        }
        
        // Represents the property CSCreationClassName
        public virtual string CSCreationClassName {
            get {
                return this._cscreationclassname;
            }
            set {
                this._cscreationclassname = value;
            }
        }
        
        // Represents the property CSName
        public virtual string CSName {
            get {
                return this._csname;
            }
            set {
                this._csname = value;
            }
        }
        
        // Represents the property Description
        public virtual string Description {
            get {
                return this._description;
            }
            set {
                this._description = value;
            }
        }
        
        // Represents the property ExecutablePath
        public virtual string ExecutablePath {
            get {
                return this._executablepath;
            }
            set {
                this._executablepath = value;
            }
        }
        
        // Represents the property ExecutionState
        public virtual ushort ExecutionState {
            get {
                return this._executionstate;
            }
            set {
                this._executionstate = value;
            }
        }
        
        // Represents the property Handle
        public virtual string Handle {
            get {
                return this._handle;
            }
            set {
                this._handle = value;
            }
        }
        
        // Represents the property HandleCount
        public virtual uint HandleCount {
            get {
                return this._handlecount;
            }
            set {
                this._handlecount = value;
            }
        }
        
        // Represents the property InstallDate
        public virtual System.DateTime InstallDate {
            get {
                return this._installdate;
            }
            set {
                this._installdate = value;
            }
        }
        
        // Represents the property KernelModeTime
        public virtual ulong KernelModeTime {
            get {
                return this._kernelmodetime;
            }
            set {
                this._kernelmodetime = value;
            }
        }
        
        // Represents the property MaximumWorkingSetSize
        public virtual uint MaximumWorkingSetSize {
            get {
                return this._maximumworkingsetsize;
            }
            set {
                this._maximumworkingsetsize = value;
            }
        }
        
        // Represents the property MinimumWorkingSetSize
        public virtual uint MinimumWorkingSetSize {
            get {
                return this._minimumworkingsetsize;
            }
            set {
                this._minimumworkingsetsize = value;
            }
        }
        
        // Represents the property Name
        public virtual string Name {
            get {
                return this._name;
            }
            set {
                this._name = value;
            }
        }
        
        // Represents the property OSCreationClassName
        public virtual string OSCreationClassName {
            get {
                return this._oscreationclassname;
            }
            set {
                this._oscreationclassname = value;
            }
        }
        
        // Represents the property OSName
        public virtual string OSName {
            get {
                return this._osname;
            }
            set {
                this._osname = value;
            }
        }
        
        // Represents the property OtherOperationCount
        public virtual ulong OtherOperationCount {
            get {
                return this._otheroperationcount;
            }
            set {
                this._otheroperationcount = value;
            }
        }
        
        // Represents the property OtherTransferCount
        public virtual ulong OtherTransferCount {
            get {
                return this._othertransfercount;
            }
            set {
                this._othertransfercount = value;
            }
        }
        
        // Represents the property PageFaults
        public virtual uint PageFaults {
            get {
                return this._pagefaults;
            }
            set {
                this._pagefaults = value;
            }
        }
        
        // Represents the property PageFileUsage
        public virtual uint PageFileUsage {
            get {
                return this._pagefileusage;
            }
            set {
                this._pagefileusage = value;
            }
        }
        
        // Represents the property ParentProcessId
        public virtual uint ParentProcessId {
            get {
                return this._parentprocessid;
            }
            set {
                this._parentprocessid = value;
            }
        }
        
        // Represents the property PeakPageFileUsage
        public virtual uint PeakPageFileUsage {
            get {
                return this._peakpagefileusage;
            }
            set {
                this._peakpagefileusage = value;
            }
        }
        
        // Represents the property PeakVirtualSize
        public virtual ulong PeakVirtualSize {
            get {
                return this._peakvirtualsize;
            }
            set {
                this._peakvirtualsize = value;
            }
        }
        
        // Represents the property PeakWorkingSetSize
        public virtual uint PeakWorkingSetSize {
            get {
                return this._peakworkingsetsize;
            }
            set {
                this._peakworkingsetsize = value;
            }
        }
        
        // Represents the property Priority
        public virtual uint Priority {
            get {
                return this._priority;
            }
            set {
                this._priority = value;
            }
        }
        
        // Represents the property PrivatePageCount
        public virtual ulong PrivatePageCount {
            get {
                return this._privatepagecount;
            }
            set {
                this._privatepagecount = value;
            }
        }
        
        // Represents the property ProcessId
        public virtual uint ProcessId {
            get {
                return this._processid;
            }
            set {
                this._processid = value;
            }
        }
        
        // Represents the property QuotaNonPagedPoolUsage
        public virtual uint QuotaNonPagedPoolUsage {
            get {
                return this._quotanonpagedpoolusage;
            }
            set {
                this._quotanonpagedpoolusage = value;
            }
        }
        
        // Represents the property QuotaPagedPoolUsage
        public virtual uint QuotaPagedPoolUsage {
            get {
                return this._quotapagedpoolusage;
            }
            set {
                this._quotapagedpoolusage = value;
            }
        }
        
        // Represents the property QuotaPeakNonPagedPoolUsage
        public virtual uint QuotaPeakNonPagedPoolUsage {
            get {
                return this._quotapeaknonpagedpoolusage;
            }
            set {
                this._quotapeaknonpagedpoolusage = value;
            }
        }
        
        // Represents the property QuotaPeakPagedPoolUsage
        public virtual uint QuotaPeakPagedPoolUsage {
            get {
                return this._quotapeakpagedpoolusage;
            }
            set {
                this._quotapeakpagedpoolusage = value;
            }
        }
        
        // Represents the property ReadOperationCount
        public virtual ulong ReadOperationCount {
            get {
                return this._readoperationcount;
            }
            set {
                this._readoperationcount = value;
            }
        }
        
        // Represents the property ReadTransferCount
        public virtual ulong ReadTransferCount {
            get {
                return this._readtransfercount;
            }
            set {
                this._readtransfercount = value;
            }
        }
        
        // Represents the property SessionId
        public virtual uint SessionId {
            get {
                return this._sessionid;
            }
            set {
                this._sessionid = value;
            }
        }
        
        // Represents the property Status
        public virtual string Status {
            get {
                return this._status;
            }
            set {
                this._status = value;
            }
        }
        
        // Represents the property TerminationDate
        public virtual System.DateTime TerminationDate {
            get {
                return this._terminationdate;
            }
            set {
                this._terminationdate = value;
            }
        }
        
        // Represents the property ThreadCount
        public virtual uint ThreadCount {
            get {
                return this._threadcount;
            }
            set {
                this._threadcount = value;
            }
        }
        
        // Represents the property UserModeTime
        public virtual ulong UserModeTime {
            get {
                return this._usermodetime;
            }
            set {
                this._usermodetime = value;
            }
        }
        
        // Represents the property VirtualSize
        public virtual ulong VirtualSize {
            get {
                return this._virtualsize;
            }
            set {
                this._virtualsize = value;
            }
        }
        
        // Represents the property WindowsVersion
        public virtual string WindowsVersion {
            get {
                return this._windowsversion;
            }
            set {
                this._windowsversion = value;
            }
        }
        
        // Represents the property WorkingSetSize
        public virtual ulong WorkingSetSize {
            get {
                return this._workingsetsize;
            }
            set {
                this._workingsetsize = value;
            }
        }
        
        // Represents the property WriteOperationCount
        public virtual ulong WriteOperationCount {
            get {
                return this._writeoperationcount;
            }
            set {
                this._writeoperationcount = value;
            }
        }
        
        // Represents the property WriteTransferCount
        public virtual ulong WriteTransferCount {
            get {
                return this._writetransfercount;
            }
            set {
                this._writetransfercount = value;
            }
        }
    }
}
