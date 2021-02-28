
using Microsoft.Win32;

namespace helpsharp.Utility
{
    public class RegistryUtils
    {
        #region Public Methods

        public string ReadRegKeyValueData(string regRoot, string regKey, string regKeyValue)
        {
            var keyRoot = GetRegistryRoot(regRoot);
            var key = keyRoot.OpenSubKey(regKey);
            if (key != null)
            {
                try
                {
                    var valueData = key.GetValue(regKeyValue).ToString();
                    return valueData;
                }
                catch
                {
                    return null;
                }
            }
            else if (key == null)
            {
                return null;
            }

            return null;
        }

        public bool RegKeyExists(string regRoot, string regKey)
        {
            var keyRoot = GetRegistryRoot(regRoot);
            var key = keyRoot.OpenSubKey(regKey);
            if (key != null)
            {
                return true;
            }
            else if (key == null)
            {
                return false;
            }

            return false;
        }

        #endregion Public Methods

        #region Internal Methods

        internal RegistryKey GetRegistryRoot(string regRoot)
        {
            RegistryKey keyRoot;
            switch (regRoot)
            {
                case "HKEY_CLASSES_ROOT":
                    keyRoot = Registry.ClassesRoot;
                    break;

                case "HKEY_CURRENT_USER":
                    keyRoot = Registry.CurrentUser;
                    break;

                case "HKEY_LOCAL_MACHINE":
                    keyRoot = Registry.LocalMachine;
                    break;

                case "HKEY_USERS":
                    keyRoot = Registry.Users;
                    break;

                case "HKEY_CURRENT_CONFIG":
                    keyRoot = Registry.CurrentConfig;
                    break;

                case "PERFORMANCE_DATA":
                    keyRoot = Registry.PerformanceData;
                    break;

                default:
                    keyRoot = Registry.LocalMachine;
                    break;
            }

            return keyRoot;
        }

        #endregion Internal Methods
    }
}