using System;
using System.Runtime.InteropServices;
using System.Text;

namespace csharp.helper.Utility
{
    public class IniFile
    {
        #region Public Constructors

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <PARAM name="INIPath"></PARAM>
        public IniFile(string iniPath)
        {
            if (iniPath == null)
            {
                throw new ArgumentNullException("iniPath");
            }

            Path = iniPath;
        }

        #endregion Public Constructors

        #region Public Properties

        public string Path { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public string IniReadValue(string section, string key)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section");
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            var temp = new StringBuilder(255);
            var i = GetPrivateProfileString(section, key, string.Empty, temp,
                                            255, Path);
            return temp.ToString();
        }

        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Value"></PARAM>
        public void IniWriteValue(string section, string key, string value)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section");
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            WritePrivateProfileString(section, key, value, Path);
        }

        #endregion Public Methods

        #region Private Methods

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                                                          string key, string def, StringBuilder retVal,
                                                          int size, string filePath);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
                                                             string key, string val, string filePath);

        #endregion Private Methods
    }
}