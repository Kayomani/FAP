using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Controllers;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Controllers
{
    [Export(typeof(IEntityController)), Export]
    public class EntityControllerMock : IEntityController
    {
        public bool InitializeCalled { get; set; }
        public bool ShutdownCalled { get; set; }
        public bool HasChangesResult { get; set; }
        public bool SaveResult { get; set; }
        public bool SaveCalled { get; set; }
        
        
        public bool HasChanges
        {
            get { return HasChangesResult; }
        }

        public void Initialize()
        {
            InitializeCalled = true;
        }

        public bool Save()
        {
            SaveCalled = true;
            return SaveResult;
        }

        public void Shutdown()
        {
            ShutdownCalled = true;
        }
    }
}
