using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications.Services;
using System.ComponentModel.Composition;

namespace Writer.Applications.Test.Services
{
    [Export(typeof(IFileDialogService))]
    public class FileDialogServiceMock : IFileDialogService
    {
        public FileDialogResult Result { get; set; }
        public FileDialogType FileDialogType { get; private set; }
        public IEnumerable<FileType> FileTypes { get; private set; }
        public FileType DefaultFileType { get; private set; }
        public string DefaultFileName { get; private set; }


        public FileDialogResult ShowOpenFileDialog(IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
        {
            FileDialogType = FileDialogType.OpenFileDialog;
            FileTypes = fileTypes;
            DefaultFileType = defaultFileType;
            DefaultFileName = defaultFileName;
            return Result;    
        }

        public FileDialogResult ShowSaveFileDialog(IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
        {
            FileDialogType = FileDialogType.SaveFileDialog;
            FileTypes = fileTypes;
            DefaultFileType = defaultFileType;
            DefaultFileName = defaultFileName;
            return Result;
        }
    }

    public enum FileDialogType
    {
        OpenFileDialog,
        SaveFileDialog
    }
}
