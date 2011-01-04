using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Waf.Applications.Services;
using System.Waf.UnitTesting;

namespace Test.Waf.Applications.Services
{
    [TestClass]
    public class FileDialogServiceExtensionsTest
    {
        [TestMethod]
        public void ShowOpenFileDialogExtensionTest()
        {
            FileType rtfFileType = new FileType("RichText Document", ".rtf");
            FileType xpsFileType = new FileType("XPS Document", ".xps");
            IEnumerable<FileType> fileTypes = new FileType[] { rtfFileType, xpsFileType };
            string defaultFileName = "Document 1.rtf";
            FileDialogResult result = new FileDialogResult("Document 2.rtf", rtfFileType);

            MockFileDialogService service = new MockFileDialogService();
            service.Result = result;

            Assert.AreEqual(result, service.ShowOpenFileDialog(rtfFileType));
            Assert.AreEqual(rtfFileType, service.FileTypes.Single());
            AssertHelper.ExpectedException<ArgumentNullException>(() => FileDialogServiceExtensions.ShowOpenFileDialog(null, rtfFileType));
            AssertHelper.ExpectedException<ArgumentNullException>(() => service.ShowOpenFileDialog((FileType)null));

            Assert.AreEqual(result, service.ShowOpenFileDialog(rtfFileType, defaultFileName));
            Assert.AreEqual(rtfFileType, service.FileTypes.Single());
            Assert.AreEqual(defaultFileName, service.DefaultFileName);
            AssertHelper.ExpectedException<ArgumentNullException>(() => FileDialogServiceExtensions.ShowOpenFileDialog(null, rtfFileType, defaultFileName));
            AssertHelper.ExpectedException<ArgumentNullException>(() => service.ShowOpenFileDialog((FileType)null, defaultFileName));

            Assert.AreEqual(result, service.ShowOpenFileDialog(fileTypes));
            Assert.IsTrue(service.FileTypes.SequenceEqual(new FileType[] { rtfFileType, xpsFileType }));
            AssertHelper.ExpectedException<ArgumentNullException>(() => FileDialogServiceExtensions.ShowOpenFileDialog(null, fileTypes));
        }

        [TestMethod]
        public void ShowSaveFileDialogExtensionTest()
        {
            FileType rtfFileType = new FileType("RichText Document", ".rtf");
            FileType xpsFileType = new FileType("XPS Document", ".xps");
            IEnumerable<FileType> fileTypes = new FileType[] { rtfFileType, xpsFileType };
            string defaultFileName = "Document 1.rtf";
            FileDialogResult result = new FileDialogResult("Document 2.rtf", rtfFileType);

            MockFileDialogService service = new MockFileDialogService();
            service.Result = result;

            Assert.AreEqual(result, service.ShowSaveFileDialog(rtfFileType));
            Assert.AreEqual(rtfFileType, service.FileTypes.Single());
            AssertHelper.ExpectedException<ArgumentNullException>(() => FileDialogServiceExtensions.ShowSaveFileDialog(null, rtfFileType));
            AssertHelper.ExpectedException<ArgumentNullException>(() => service.ShowSaveFileDialog((FileType)null));

            Assert.AreEqual(result, service.ShowSaveFileDialog(rtfFileType, defaultFileName));
            Assert.AreEqual(rtfFileType, service.FileTypes.Single());
            Assert.AreEqual(defaultFileName, service.DefaultFileName);
            AssertHelper.ExpectedException<ArgumentNullException>(() => FileDialogServiceExtensions.ShowSaveFileDialog(null, rtfFileType, defaultFileName));
            AssertHelper.ExpectedException<ArgumentNullException>(() => service.ShowSaveFileDialog((FileType)null, defaultFileName));

            Assert.AreEqual(result, service.ShowSaveFileDialog(fileTypes));
            Assert.IsTrue(service.FileTypes.SequenceEqual(new FileType[] { rtfFileType, xpsFileType }));
            AssertHelper.ExpectedException<ArgumentNullException>(() => FileDialogServiceExtensions.ShowSaveFileDialog(null, fileTypes));
        }



        private class MockFileDialogService : IFileDialogService
        {
            public IEnumerable<FileType> FileTypes { get; private set; }
            public FileType DefaultFileType { get; private set; }
            public string DefaultFileName { get; private set; }
            public FileDialogResult Result { get; set; }


            public FileDialogResult ShowOpenFileDialog(IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
            {
                FileTypes = fileTypes;
                DefaultFileType = defaultFileType;
                DefaultFileName = defaultFileName;
                return Result;
            }

            public FileDialogResult ShowSaveFileDialog(IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
            {
                FileTypes = fileTypes;
                DefaultFileType = defaultFileType;
                DefaultFileName = defaultFileName;
                return Result;
            }
        }
    }
}
