using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using helpsharp.Security.CredentialManagement;

namespace helpsharp.Security
{
    public class CredentialManagerWrapper
    {
        #region Public Methods

        public static void DeleteProxyServerCredential()
        {
            var writeErrorCode = CredentialManager.DeleteCredentials(CredentialManagerKeys.SomeString);
            if (writeErrorCode != 0)
            {
                var message = string.Format("Failed to delete the Proxy Server credential to the Windows Credential Manager's store. ErrorCode: {0}", writeErrorCode);
                throw new Exception(message + "|| Win32 Error Code: " + Marshal.GetLastWin32Error() + "|| Message: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }
        }

        public static NetworkCredential GetProxyServerCredential()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(CredentialManagerKeys.SomeString);
                var splitDomain = string.Empty;
                var splitUserName = cred.UserName.ConvertToUnsecureString();
                if (cred.UserName.ConvertToUnsecureString().Contains("\\"))
                {
                    splitDomain = cred.UserName.ConvertToUnsecureString().Split('\\')[0];
                    splitUserName = cred.UserName.ConvertToUnsecureString().Split('\\')[1];
                }

                var networkCredential = new NetworkCredential()
                {
                    UserName = splitUserName,
                    Password = cred.Password.ConvertToUnsecureString(),
                    Domain = splitDomain
                };
                return networkCredential;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                   "Failed to get the Proxy Server credential from the Windows Credential Manager's store. Exception: " + ex.Message);
            }
        }

        public static bool ProxyServerCredentialExists()
        {
            try
            {
                SecureString userName;
                SecureString password;
                var readError = CredentialManager.ReadCredentials(CredentialManagerKeys.SomeString, out userName, out password);
                if (readError != 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                   "Failed to get the proxy server credential from the Windows Credential Manager's store. Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Setup windows credentials manager with proxy server authentication credentials
        /// </summary>
        /// <param name="nodeconfigdata"></param>
        public static void SetupProxyServerCredential(SecureString proxyServerUserName, SecureString proxyServerPassword)
        {
            var writeErrorCode = CredentialManager.WriteCredentials(CredentialManagerKeys.SomeString,
                proxyServerUserName,
                proxyServerPassword);
            if (writeErrorCode != 0)
            {
                var message = string.Format("Failed to write the Proxy Server credential to the Windows Credential Manager's store. ErrorCode: {0}", writeErrorCode);
                throw new Exception(message + "|| Win32 Error Code: " + Marshal.GetLastWin32Error() + "|| Message: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }
        }

        #endregion Public Methods
    }
}