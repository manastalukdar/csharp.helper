using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace helpsharp.Utility
{
    public class DirectoryAndFile
    {
        #region Public Methods

        /// <summary>
        /// Copies the files.
        /// </summary>
        /// <param name="sourceDir">The source dir.</param>
        /// <param name="destDir">The dest dir.</param>
        /// <param name="fileString">The file string.</param>
        /// <param name="overWrite">if set to <c>true</c> [over write].</param>
        /// <exception cref="System.ArgumentNullException">sourceDir or destDir or fileString</exception>
        public void CopyFiles(string sourceDir, string destDir, string fileString, bool overWrite)
        {
            if (sourceDir == null)
            {
                throw new ArgumentNullException("sourceDir");
            }

            if (destDir == null)
            {
                throw new ArgumentNullException("destDir");
            }

            if (fileString == null)
            {
                throw new ArgumentNullException("fileString");
            }

            var source = new DirectoryInfo(sourceDir);
            var destination = new DirectoryInfo(destDir);
            if (!destination.Exists)
            {
                destination.Create();
            }

            var files = source.GetFiles();
            foreach (var fileInfo in files.Where(fileInfo => fileInfo.Name.Contains(fileString)))
            {
                fileInfo.CopyTo(Path.Combine(destination.FullName,
                                           fileInfo.Name), overWrite);
            }
        }

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <exception cref="System.ArgumentNullException">dir</exception>
        public void CreateDirectory(string dir)
        {
            if (dir == null)
            {
                throw new ArgumentNullException("dir");
            }

            var dirInfo = new DirectoryInfo(dir);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }

        /// <summary>
        /// Deletes the dir.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <exception cref="System.ArgumentNullException">dir</exception>
        public void DeleteDir(string dir, bool recursive)
        {
            if (dir == null)
            {
                throw new ArgumentNullException("dir");
            }

            Directory.Delete(dir, recursive);
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="filePathAndName">Name of the file path and.</param>
        /// <exception cref="System.ArgumentNullException">filePathAndName</exception>
        public void DeleteFile(string filePathAndName)
        {
            if (filePathAndName == null)
            {
                throw new ArgumentNullException("filePathAndName");
            }

            File.Delete(filePathAndName);
        }

        /// <summary>
        /// Checks if directory exists.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">dir</exception>
        public bool DirExists(string dir)
        {
            if (dir == null)
            {
                throw new ArgumentNullException("dir");
            }

            return Directory.Exists(dir);
        }

        /// <summary>
        /// Checks if file exists.
        /// </summary>
        /// <param name="filePathAndName">Name of the file path and.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">filePathAndName</exception>
        public bool FileExists(string filePathAndName)
        {
            if (filePathAndName == null)
            {
                throw new ArgumentNullException("filePathAndName");
            }

            return File.Exists(filePathAndName);
        }

        /// <summary>
        /// Matches passed file versions.
        /// </summary>
        /// <param name="filePathAndName">Name of the file path and.</param>
        /// <param name="expectedVersion">The expected version.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">filePathAndName or expectedVersion</exception>
        public bool FileVersionMatches(string filePathAndName, string expectedVersion)
        {
            if (filePathAndName == null)
            {
                throw new ArgumentNullException("filePathAndName");
            }

            if (expectedVersion == null)
            {
                throw new ArgumentNullException("expectedVersion");
            }

            var strActualVersion = GetFileVersion(filePathAndName);
            return Equals(strActualVersion, expectedVersion);
        }

        /// <summary>
        /// Gets the file version.
        /// </summary>
        /// <param name="filePathAndName">Name of the file path and.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">filePathAndName</exception>
        public string GetFileVersion(string filePathAndName)
        {
            if (filePathAndName == null)
            {
                throw new ArgumentNullException("filePathAndName");
            }

            var fviFileVersion = FileVersionInfo.GetVersionInfo(filePathAndName);
            var fileVersion = fviFileVersion.FileVersion;
            return fileVersion;
        }

        /// <summary>
        /// Moves the files.
        /// </summary>
        /// <param name="sourceDir">The source dir.</param>
        /// <param name="destDir">The dest dir.</param>
        /// <param name="fileString">The file string.</param>
        /// <exception cref="System.ArgumentNullException">sourceDir or destDir or fileString</exception>
        public void MoveFiles(string sourceDir, string destDir, string fileString)
        {
            if (sourceDir == null)
            {
                throw new ArgumentNullException("sourceDir");
            }

            if (destDir == null)
            {
                throw new ArgumentNullException("destDir");
            }

            if (fileString == null)
            {
                throw new ArgumentNullException("fileString");
            }

            var dirInfoSource = new DirectoryInfo(sourceDir);
            var dirInfoDestination = new DirectoryInfo(destDir);
            if (!dirInfoDestination.Exists)
            {
                dirInfoDestination.Create();
            }

            var files = dirInfoSource.GetFiles();
            foreach (var fileInfo in files.Where(fileInfo => fileInfo.Name.Contains(fileString)))
            {
                fileInfo.MoveTo(Path.Combine(dirInfoDestination.FullName,
                                           fileInfo.Name));
            }
        }

        #endregion Public Methods

        #region Security

        /// <summary>
        /// Adds the directory security.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="account">The account - user or group.</param>
        /// <param name="rights">The rights.</param>
        /// <param name="controlType">Type of the control.</param>
        public static void AddDirectorySecurity(string folderName, string account, FileSystemRights rights, InheritanceFlags inheritenceFlags, PropagationFlags propagationFlags, AccessControlType controlType)
        {
            // Create a new DirectoryInfo object.
            var dInfo = new DirectoryInfo(folderName);

            // Get a DirectorySecurity object that represents the current security settings.
            var dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings.
            dSecurity.AddAccessRule(new FileSystemAccessRule(account, rights, inheritenceFlags, propagationFlags, controlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);
        }

        // <summary>
        // Removes the directory security.
        // </summary>
        // <param name="FolderName">Name of the folder.</param>
        // <param name="Account">The account - user or group.</param>
        // <param name="Rights">The rights.</param>
        // <param name="ControlType">Type of the control.</param>
        public static void RemoveDirectorySecurity(string folderName, string account, FileSystemRights rights, InheritanceFlags inheritenceFlags, PropagationFlags propagationFlags, AccessControlType controlType)
        {
            // Create a new DirectoryInfo object.
            var dInfo = new DirectoryInfo(folderName);

            // Get a DirectorySecurity object that represents the current security settings.
            var dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings.
            dSecurity.RemoveAccessRule(new FileSystemAccessRule(account, rights, inheritenceFlags, propagationFlags, controlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);
        }

        #endregion Security
    }
}