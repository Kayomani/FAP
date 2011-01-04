using System.Collections.Generic;

namespace System.Waf.Applications.Services
{
    /// <summary>
    /// Provides method overloads for the <see cref="IFileDialogService"/> to simplify its usage.
    /// </summary>
    public static class FileDialogServiceExtensions
    {
        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="ArgumentNullException">service must not be null.</exception>
        /// <exception cref="ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IFileDialogService service, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(new FileType[] { fileType }, fileType, null);
        }

        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <param name="defaultFileName">Default filename.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="ArgumentNullException">service must not be null.</exception>
        /// <exception cref="ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IFileDialogService service, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(new FileType[] { fileType }, fileType, defaultFileName);
        }

        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileTypes">The supported file types.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="ArgumentNullException">service must not be null.</exception>
        /// <exception cref="ArgumentNullException">fileTypes must not be null.</exception>
        /// <exception cref="ArgumentException">fileTypes must contain at least one item.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IFileDialogService service, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowOpenFileDialog(fileTypes, null, null);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="ArgumentNullException">service must not be null.</exception>
        /// <exception cref="ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IFileDialogService service, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(new FileType[] { fileType }, fileType, null);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <param name="defaultFileName">Default filename.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="ArgumentNullException">service must not be null.</exception>
        /// <exception cref="ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IFileDialogService service, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(new FileType[] { fileType }, fileType, defaultFileName);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileTypes">The supported file types.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="ArgumentNullException">service must not be null.</exception>
        /// <exception cref="ArgumentNullException">fileTypes must not be null.</exception>
        /// <exception cref="ArgumentException">fileTypes must contain at least one item.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IFileDialogService service, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowSaveFileDialog(fileTypes, null, null);
        }
    }
}
