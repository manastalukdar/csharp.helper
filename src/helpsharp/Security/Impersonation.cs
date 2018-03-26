using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using helpsharp.Utility;

namespace helpsharp.Security
{
    public class Impersonation
    {
        #region Private Fields

        private static WindowsImpersonationContext _impersonationContext;

        #endregion Private Fields

        #region Public Enums

        [Flags]
        public enum DesiredAccess
        {
            StandardRightsRequired = 0x000F0000,
            StandardRightsRead = 0x00020000,
            TokenAssignPrimary = 0x0001,
            TokenDuplicate = 0x0002,
            TokenImpersonate = 0x0004,
            TokenQuery = 0x0008,
            TokenQuerySource = 0x0010,
            TokenAdjustPrivileges = 0x0020,
            TokenAdjustGroups = 0x0040,
            TokenAdjustDefault = 0x0080,
            TokenAdjustSessionid = 0x0100,
            TokenRead = (StandardRightsRead | TokenQuery),

            TokenAllAccess = (StandardRightsRequired | TokenAssignPrimary |
                TokenDuplicate | TokenImpersonate | TokenQuery | TokenQuerySource |
                TokenAdjustPrivileges | TokenAdjustGroups | TokenAdjustDefault |
                TokenAdjustSessionid)
        }

        [Flags]
        public enum LogonProvider
        {
            /// <summary>
            /// Use the standard logon provider for the system. The default security provider is
            /// negotiate, unless you pass NULL for the domain name and the user name is not in UPN
            /// format. In this case, the default provider is NTLM.
            /// NOTE: Windows 2000/NT: The default security provider is NTLM.
            /// </summary>
            Logon32ProviderDefault = 0,

            Logon32ProviderWinnt35 = 1,
            Logon32ProviderWinnt40 = 2,
            Logon32ProviderWinnt50 = 3
        }

        [Flags]
        public enum LogonType
        {
            /// <summary>
            /// This logon type is intended for users who will be interactively using the computer,
            /// such as a user being logged on by a terminal server, remote shell, or similar
            /// process. This logon type has the additional expense of caching logon information for
            /// disconnected operations; therefore, it is inappropriate for some client/server
            /// applications, such as a mail server.
            /// </summary>
            Logon32LogonInteractive = 2,

            /// <summary>
            /// This logon type is intended for high performance servers to authenticate plaintext
            /// passwords. The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            Logon32LogonNetwork = 3,

            /// <summary>
            /// This logon type is intended for batch servers, where processes may be executing on
            /// behalf of a user without their direct intervention. This type is also for higher
            /// performance servers that process many plaintext authentication attempts at a time,
            /// such as mail or Web servers. The LogonUser function does not cache credentials for
            /// this logon type.
            /// </summary>
            Logon32LogonBatch = 4,

            /// <summary>
            /// Indicates a service-type logon. The account provided must have the service privilege enabled.
            /// </summary>
            Logon32LogonService = 5,

            /// <summary>
            /// This logon type is for GINA DLLs that log on users who will be interactively using
            /// the computer. This logon type can generate a unique audit record that shows when the
            /// workstation was unlocked.
            /// </summary>
            Logon32LogonUnlock = 7,

            /// <summary>
            /// This logon type preserves the name and password in the authentication package, which
            /// allows the server to make connections to other network servers while impersonating
            /// the client. A server can accept plaintext credentials from a client, call LogonUser,
            /// verify that the user can access the system across the network, and still communicate
            /// with other servers.
            /// NOTE: Windows NT: This value is not supported.
            /// </summary>
            Logon32LogonNetworkCleartext = 8,

            /// <summary>
            /// This logon type allows the caller to clone its current token and specify new
            /// credentials for outbound connections. The new logon session has the same local
            /// identifier but uses different credentials for other network connections.
            /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
            /// NOTE: Windows NT: This value is not supported.
            /// </summary>
            Logon32LogonNewCredentials = 9,
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VmOperation = 0x00000008,
            VmRead = 0x00000010,
            VmWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [Flags]
        public enum SecurityImpersonationLevel
        {
            /// <summary>
            /// The server process cannot obtain identification information about the client, and it
            /// cannot impersonate the client. It is defined with no value given, and thus, by ANSI
            /// C rules, defaults to a value of zero.
            /// </summary>
            SecurityAnonymous = 0,

            /// <summary>
            /// The server process can obtain information about the client, such as security
            /// identifiers and privileges, but it cannot impersonate the client. This is useful for
            /// servers that export their own objects, for example, database products that export
            /// tables and views. Using the retrieved client-security information, the server can
            /// make access-validation decisions without being able to use other services that are
            /// using the client's security context.
            /// </summary>
            SecurityIdentification = 1,

            /// <summary>
            /// The server process can impersonate the client's security context on its local
            /// system. The server cannot impersonate the client on remote systems.
            /// </summary>
            SecurityImpersonation = 2,

            /// <summary>
            /// The server process can impersonate the client's security context on remote systems.
            /// NOTE: Windows NT: This impersonation level is not supported.
            /// </summary>
            SecurityDelegation = 3,
        }

        #endregion Public Enums

        #region Public Methods

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DuplicateToken(IntPtr existingTokenHandle, int securityImpersonationLevel,
                                                 out IntPtr duplicateTokenHandle);

        /// <summary>
        /// Impersonates a passed Windows Identity
        /// </summary>
        /// <param name="identity"></param>
        public static void Impersonate(WindowsIdentity identity, bool loadProfile)
        {
            if (loadProfile)
            {
                var systemUtil = new SystemUtils();
                var domainAndUsername = new string[2];
                var utilsMisc = new Miscellaneous();
                domainAndUsername = utilsMisc.FormatDomainAndUsername(identity.Name);
                var userName = domainAndUsername[1];
                if (!systemUtil.LoadProfile(identity.Token, userName))
                {
                    throw new Exception("LoadProfile failed. " + "Win32 Error Code: " + Marshal.GetLastWin32Error() + "|| Message: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
                }
            }

            _impersonationContext = identity.Impersonate();
            if (_impersonationContext == null)
            {
                throw new Exception("__impersonationContext is null");
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(
           string lpszUsername,
           string lpszDomain,
           string lpszPassword,
           int logonType,
           int logonProvider,
           out IntPtr token);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(
           string lpszUsername,
           string lpszDomain,
           string lpszPassword,
           int logonType,
           int logonProvider,
           out SafeTokenHandle token);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags desiredAccess,
                                                [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        /// <summary>
        /// Unimpersonate
        /// </summary>
        public static void UnImpersonate()
        {
            if (_impersonationContext == null)
            {
                // [Manas]: log this
                ////throw new Exception("__impersonationContext is null");
            }
            else
            {
                _impersonationContext.Undo();
            }
        }

        #endregion Public Methods
    }
}