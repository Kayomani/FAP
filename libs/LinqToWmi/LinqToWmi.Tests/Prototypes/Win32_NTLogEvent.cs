namespace WmiEntities {
    using System;
    using System.Linq;
    using LinqToWmi.Core.WMI;
    
    
    public class Win32_NTLogEvent {
        
        private ushort _category;
        
        private string _categorystring;
        
        private string _computername;
        
        private byte[] _data;
        
        private ushort _eventcode;
        
        private uint _eventidentifier;
        
        private byte _eventtype;
        
        private string[] _insertionstrings;
        
        private string _logfile;
        
        private string _message;
        
        private uint _recordnumber;
        
        private string _sourcename;
        
        private System.DateTime _timegenerated;
        
        private System.DateTime _timewritten;
        
        private string _type;
        
        private string _user;
        
        // Represents the property Category
        public virtual ushort Category {
            get {
                return this._category;
            }
            set {
                this._category = value;
            }
        }
        
        // Represents the property CategoryString
        public virtual string CategoryString {
            get {
                return this._categorystring;
            }
            set {
                this._categorystring = value;
            }
        }
        
        // Represents the property ComputerName
        public virtual string ComputerName {
            get {
                return this._computername;
            }
            set {
                this._computername = value;
            }
        }
        
        // Represents the property Data
        public virtual byte[] Data {
            get {
                return this._data;
            }
            set {
                this._data = value;
            }
        }
        
        // Represents the property EventCode
        public virtual ushort EventCode {
            get {
                return this._eventcode;
            }
            set {
                this._eventcode = value;
            }
        }
        
        // Represents the property EventIdentifier
        public virtual uint EventIdentifier {
            get {
                return this._eventidentifier;
            }
            set {
                this._eventidentifier = value;
            }
        }
        
        // Represents the property EventType
        public virtual byte EventType {
            get {
                return this._eventtype;
            }
            set {
                this._eventtype = value;
            }
        }
        
        // Represents the property InsertionStrings
        public virtual string[] InsertionStrings {
            get {
                return this._insertionstrings;
            }
            set {
                this._insertionstrings = value;
            }
        }
        
        // Represents the property Logfile
        public virtual string Logfile {
            get {
                return this._logfile;
            }
            set {
                this._logfile = value;
            }
        }
        
        // Represents the property Message
        public virtual string Message {
            get {
                return this._message;
            }
            set {
                this._message = value;
            }
        }
        
        // Represents the property RecordNumber
        public virtual uint RecordNumber {
            get {
                return this._recordnumber;
            }
            set {
                this._recordnumber = value;
            }
        }
        
        // Represents the property SourceName
        public virtual string SourceName {
            get {
                return this._sourcename;
            }
            set {
                this._sourcename = value;
            }
        }
        
        // Represents the property TimeGenerated
        public virtual System.DateTime TimeGenerated {
            get {
                return this._timegenerated;
            }
            set {
                this._timegenerated = value;
            }
        }
        
        // Represents the property TimeWritten
        public virtual System.DateTime TimeWritten {
            get {
                return this._timewritten;
            }
            set {
                this._timewritten = value;
            }
        }
        
        // Represents the property Type
        public virtual string Type {
            get {
                return this._type;
            }
            set {
                this._type = value;
            }
        }
        
        // Represents the property User
        public virtual string User {
            get {
                return this._user;
            }
            set {
                this._user = value;
            }
        }
    }
}
