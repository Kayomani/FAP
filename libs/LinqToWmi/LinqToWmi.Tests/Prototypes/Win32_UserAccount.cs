namespace WmiEntities {
    using System;
    using System.Linq;
    using LinqToWmi.Core.WMI;
    
    
    public class Win32_UserAccount {
        
        private uint _accounttype;
        
        private string _caption;
        
        private string _description;
        
        private bool _disabled;
        
        private string _domain;
        
        private string _fullname;
        
        private System.DateTime _installdate;
        
        private bool _localaccount;
        
        private bool _lockout;
        
        private string _name;
        
        private bool _passwordchangeable;
        
        private bool _passwordexpires;
        
        private bool _passwordrequired;
        
        private string _sid;
        
        private byte _sidtype;
        
        private string _status;
        
        // Represents the property AccountType
        public virtual uint AccountType {
            get {
                return this._accounttype;
            }
            set {
                this._accounttype = value;
            }
        }
        
        // Represents the property Caption
        public virtual string Caption {
            get {
                return this._caption;
            }
            set {
                this._caption = value;
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
        
        // Represents the property Disabled
        public virtual bool Disabled {
            get {
                return this._disabled;
            }
            set {
                this._disabled = value;
            }
        }
        
        // Represents the property Domain
        public virtual string Domain {
            get {
                return this._domain;
            }
            set {
                this._domain = value;
            }
        }
        
        // Represents the property FullName
        public virtual string FullName {
            get {
                return this._fullname;
            }
            set {
                this._fullname = value;
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
        
        // Represents the property LocalAccount
        public virtual bool LocalAccount {
            get {
                return this._localaccount;
            }
            set {
                this._localaccount = value;
            }
        }
        
        // Represents the property Lockout
        public virtual bool Lockout {
            get {
                return this._lockout;
            }
            set {
                this._lockout = value;
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
        
        // Represents the property PasswordChangeable
        public virtual bool PasswordChangeable {
            get {
                return this._passwordchangeable;
            }
            set {
                this._passwordchangeable = value;
            }
        }
        
        // Represents the property PasswordExpires
        public virtual bool PasswordExpires {
            get {
                return this._passwordexpires;
            }
            set {
                this._passwordexpires = value;
            }
        }
        
        // Represents the property PasswordRequired
        public virtual bool PasswordRequired {
            get {
                return this._passwordrequired;
            }
            set {
                this._passwordrequired = value;
            }
        }
        
        // Represents the property SID
        public virtual string SID {
            get {
                return this._sid;
            }
            set {
                this._sid = value;
            }
        }
        
        // Represents the property SIDType
        public virtual byte SIDType {
            get {
                return this._sidtype;
            }
            set {
                this._sidtype = value;
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
    }
}
