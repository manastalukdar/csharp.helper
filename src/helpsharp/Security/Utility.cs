using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

namespace helpsharp.Security
{
    public class Utility
    {
        #region Public Methods

        public IntPtr GetPrimaryToken(string userName, string domain, SecureString password)
        {
            const int logon32ProviderDefault = 0;
            const int logon32LogonInteractive = 2;
            const int logon32LogonNetworkCleartext = 8;
            var pass = password.ConvertToUnsecureString();
            if (domain == Environment.MachineName)
                domain = ".";

            if (
                !Impersonation.LogonUser(userName, domain, pass, logon32LogonInteractive, logon32ProviderDefault,
                    out IntPtr token))
            {
                if (Marshal.GetLastWin32Error() == 1385)
                {
                    if (
                        !Impersonation.LogonUser(userName, domain, pass, logon32LogonNetworkCleartext,
                            logon32ProviderDefault, out token))
                    {
                        throw new ApplicationException("Could not get token.  Win32 Error Code: " +
                                                      Marshal.GetLastWin32Error() + "|| Message: " +
                                                      new Win32Exception(Marshal.GetLastWin32Error()).Message);
                    }
                }
                else
                {
                    throw new ApplicationException("Could not get token.  Win32 Error Code: " +
                                                   Marshal.GetLastWin32Error() + "|| Message: " +
                                                   new Win32Exception(Marshal.GetLastWin32Error()).Message);
                }
            }

            // write to log here
            var securityImpersonation =
                Convert.ToInt32(Impersonation.SecurityImpersonationLevel.SecurityImpersonation);
            if (!Impersonation.DuplicateToken(token, securityImpersonation, out IntPtr dupTokenHandle))
            {
                throw new ApplicationException("Duplication of token failed. Win32 Error Code: " +
                                              Marshal.GetLastWin32Error() + "|| Message: " +
                                              new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }

            // write to log here
            if (!Impersonation.CloseHandle(token))
            {
                throw new ApplicationException("Could not close token handle.  Win32 Error Code: " +
                                              Marshal.GetLastWin32Error() + "|| Message: " +
                                              new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }

            return dupTokenHandle;
        }

        public SafeTokenHandle GetSafeToken(string userName, string domain, SecureString password)
        {
            const int logon32ProviderDefault = 0;
            const int logon32LogonInteractive = 2;
            const int logon32LogonNetworkCleartext = 8;
            var pass = password.ConvertToUnsecureString();
            if (domain == Environment.MachineName)
                domain = ".";

            if (Impersonation.LogonUser(userName, domain, pass,
                logon32LogonInteractive, logon32ProviderDefault,
                out SafeTokenHandle safeTokenHandle))
            {
                return safeTokenHandle;
            }

            if (Marshal.GetLastWin32Error() == 1385)
            {
                if (!Impersonation.LogonUser(userName, domain, pass,
                    logon32LogonNetworkCleartext, logon32ProviderDefault,
                    out safeTokenHandle))
                {
                    throw new ApplicationException("Could not get token.  Win32 Error Code: " +
                                                  Marshal.GetLastWin32Error() + "|| Message: " +
                                                  new Win32Exception(Marshal.GetLastWin32Error()).Message);
                }
            }
            else
            {
                throw new ApplicationException("Could not get token.  Win32 Error Code: " +
                                               Marshal.GetLastWin32Error() + "|| Message: " +
                                               new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }

            return safeTokenHandle;
        }

        ////[PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public IntPtr GetToken(string userName, string domain, SecureString password)
        {
            const int logon32ProviderDefault = 0;
            const int logon32LogonInteractive = 2;
            const int logon32LogonNetworkCleartext = 8;
            var pass = password.ConvertToUnsecureString();
            if (domain == Environment.MachineName)
                domain = ".";

            if (
                !Impersonation.LogonUser(userName, domain, pass, logon32LogonInteractive, logon32ProviderDefault,
                    out IntPtr token))
            {
                if (Marshal.GetLastWin32Error() == 1385)
                {
                    if (
                        !Impersonation.LogonUser(userName, domain, pass, logon32LogonNetworkCleartext,
                            logon32ProviderDefault, out token))
                    {
                        throw new ApplicationException("Could not get token.  Win32 Error Code: " +
                                                      Marshal.GetLastWin32Error() + "|| Message: " +
                                                      new Win32Exception(Marshal.GetLastWin32Error()).Message);
                    }
                }
                else
                {
                    throw new ApplicationException("Could not get token.  Win32 Error Code: " +
                                                   Marshal.GetLastWin32Error() + "|| Message: " +
                                                   new Win32Exception(Marshal.GetLastWin32Error()).Message);
                }
            }

            // write to log here
            var securityImpersonation =
                Convert.ToInt32(Impersonation.SecurityImpersonationLevel.SecurityImpersonation);
            if (!Impersonation.DuplicateToken(token, securityImpersonation, out IntPtr dupTokenHandle))
            {
                throw new ApplicationException("Duplication of token failed. Win32 Error Code: " +
                                              Marshal.GetLastWin32Error() + "|| Message: " +
                                              new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }

            // write to log here
            if (!Impersonation.CloseHandle(dupTokenHandle))
            {
                throw new ApplicationException("Could not close duplicate token handle.  Win32 Error Code: " +
                                              Marshal.GetLastWin32Error() + "|| Message: " +
                                              new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }

            return token;
        }

        public bool IsLogonUserInteractiveDisabled(string userName, string domain, SecureString password)
        {
            const int logon32ProviderDefault = 0;
            const int logon32LogonInteractive = 2;
            var pass = password.ConvertToUnsecureString();
            if (domain == Environment.MachineName)
                domain = ".";

            if (!Impersonation.LogonUser(userName, domain, pass, logon32LogonInteractive, logon32ProviderDefault,
                    out IntPtr token) && Marshal.GetLastWin32Error() == 1385)
            {
                return true;
            }

            if (token == IntPtr.Zero) return false;
            if (!Impersonation.CloseHandle(token))
            {
                throw new ApplicationException("Could not close token handle.  Win32 Error Code: " +
                                              Marshal.GetLastWin32Error() + "|| Message: " +
                                              new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }

            return false;
        }

        #endregion Public Methods
    }
}