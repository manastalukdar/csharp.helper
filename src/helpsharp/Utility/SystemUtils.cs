using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using WUApiLib;

namespace helpsharp.Utility
{
    public class SystemUtils
    {
        #region Private Fields

        private Profileinfo _profileInfo;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Collect User Info
        /// </summary>
        /// <param name="pToken">Process Handle</param>
        public static bool DumpUserInfo(IntPtr pToken, out IntPtr sid)
        {
            const uint access = (uint)DesiredAccess.TokenQuery;
            var ret = false;
            sid = IntPtr.Zero;
            try
            {
                IntPtr procToken;
                if (!OpenProcessToken(pToken, access, out procToken)) return false;
                ret = ProcessTokenToSid(procToken, out sid);
                CloseHandle(procToken);
                return ret;
            }
            catch (Exception err)
            {
                Console.WriteLine("Method [" + new StackFrame(0).GetMethod().Name + "]. Error " + err.Message);
                return false;
            }
        }

        public static string ExGetProcessInfoByPid(int pid, out string sid)//, out string OwnerSID)
        {
            sid = string.Empty;
            try
            {
                var process = Process.GetProcessById(pid);
                if (DumpUserInfo(process.Handle, out IntPtr intPtrDSid))
                {
                    ConvertSidToStringSid(intPtrDSid, ref sid);
                }
                return process.ProcessName;
            }
            catch
            {
                return "Unknown";
            }
        }

        public WindowsIdentity GetProcessUser()
        {
            var result = false;
            IntPtr tokenHandle;
            var currentProcess = Process.GetCurrentProcess();
            result = OpenProcessToken(currentProcess.Handle, (uint)DesiredAccess.TokenAllAccess, out tokenHandle);
            if (!result)
            {
                var error = "Win32 Error Code: " + Marshal.GetLastWin32Error() + "|| Message: " + new Win32Exception(Marshal.GetLastWin32Error()).Message;
                throw new Exception(error);
            }

            return new WindowsIdentity(tokenHandle);
        }

        public bool IsAdmin()
        {
            var id = WindowsIdentity.GetCurrent();
            var p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public bool IsAdmin(WindowsIdentity identity)
        {
            var p = new WindowsPrincipal(identity);
            //return p.IsInRole(WindowsBuiltInRole.Administrator);
            return p.Claims.Any((c) => c.Value == "S-1-5-32-544");
        }

        /// <summary>
        /// Determines whether [is node32 bit].
        /// </summary>
        /// <returns><c>true</c> if [is node32 bit]; otherwise, <c>false</c>.</returns>
        public bool IsNode32Bit()
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is node64 bit].
        /// </summary>
        /// <returns><c>true</c> if [is node64 bit]; otherwise, <c>false</c>.</returns>
        public bool IsNode64Bit()
        {
            return Environment.Is64BitOperatingSystem;
        }

        public bool IsRebootPending()
        {
            var sysInfo = new SystemInformation();
            return sysInfo.RebootRequired;
        }

        public bool LaunchElevatedProcess(IntPtr parentHandle, string fullyQualifiedName, string arguments)
        {
            Debug.WriteLine("Entering Elevation.LaunchElevatedProcess");

            var workingDirectory = string.Empty;
            var fileName = string.Empty;
            if (fullyQualifiedName.Contains("\\"))
            {
                var index = fullyQualifiedName.LastIndexOf('\\');
                workingDirectory = fullyQualifiedName.Substring(0, index);
                if (fullyQualifiedName.Length > index + 1)
                {
                    fileName = fullyQualifiedName.Substring(index + 1);
                }
            }

            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true; // default, but be explicit
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.FileName = fileName;
            startInfo.Verb = "runas";
            startInfo.Arguments = arguments;

            // Two lines below make the UAC dialog modal to this app
            startInfo.ErrorDialog = true;
            startInfo.ErrorDialogParentHandle = parentHandle;

            try
            {
                Debug.WriteLine("Calling Process.Start(" + startInfo.Arguments + ")");
                Process.Start(startInfo);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool LoadProfile(IntPtr token, string userName)
        {
            _profileInfo = new Profileinfo();
            _profileInfo.Size = Marshal.SizeOf(_profileInfo);
            _profileInfo.Flags = 1;
            _profileInfo.UserName = userName;
            return LoadUserProfile(token, ref _profileInfo);
        }

        public bool LoadProfileOfUserProcessRunningUnder()
        {
            var result = false;
            var currentProcess = Process.GetCurrentProcess();
            //// IntPtr currentProcess = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VMRead, false, (int)currProcess.Id);
            OpenProcessToken(currentProcess.Handle, (uint)DesiredAccess.TokenAllAccess, out IntPtr tokenHandle);
            var wi = new WindowsIdentity(tokenHandle);
            var utilsMisc = new Miscellaneous();
            var domainAndUsername = utilsMisc.FormatDomainAndUsername(wi.Name);
            var userName = domainAndUsername[1];
            result = LoadProfile(tokenHandle, userName);
            if (!result)
            {
                var error = "Win32 Error Code: " + Marshal.GetLastWin32Error() + "|| Message: " + new Win32Exception(Marshal.GetLastWin32Error()).Message;
                throw new Exception(error);
            }

            result = CloseHandle(tokenHandle);

            return result;
        }

        public bool UnloadProfile(IntPtr token)
        {
            return UnloadUserProfile(token, _profileInfo.Profile);
        }

        #endregion Public Methods

        #region Private Methods

        private static bool ProcessTokenToSid(IntPtr token, out IntPtr sid)
        {
            const int bufLength = 256;
            var tu = Marshal.AllocHGlobal(bufLength);
            sid = IntPtr.Zero;
            try
            {
                var cb = bufLength;
                var ret = GetTokenInformation(token,
                    TOKEN_INFORMATION_CLASS.TokenUser, tu, cb, ref cb);
                if (!ret) return ret;
                var tokUser = (TokenUser)Marshal.PtrToStructure(tu, typeof(TokenUser));
                sid = tokUser.User.Sid;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(tu);
            }
        }

        #endregion Private Methods

        #region Native Calls (P/Invoke and data structures)

        [Flags]
        private enum DesiredAccess : uint
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
            TokenRead = StandardRightsRead | TokenQuery,

            TokenAllAccess = StandardRightsRequired | TokenAssignPrimary |
                TokenDuplicate | TokenImpersonate | TokenQuery | TokenQuerySource |
                TokenAdjustPrivileges | TokenAdjustGroups | TokenAdjustDefault |
                TokenAdjustSessionid
        }

        [Flags]
        private enum ProcessAccessFlags : uint
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
        private enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32", CharSet = CharSet.Auto)]
        private static extern bool ConvertSidToStringSid(
            IntPtr pSid,
            [In, Out, MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid
        );

        [DllImport("advapi32", CharSet = CharSet.Auto)]
        private static extern bool ConvertStringSidToSid(
            [In, MarshalAs(UnmanagedType.LPTStr)] string pStringSid,
            ref IntPtr pSid
        );

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32", CharSet = CharSet.Auto)]
        private static extern bool GetTokenInformation(
            IntPtr hToken,
            TOKEN_INFORMATION_CLASS tokenInfoClass,
            IntPtr tokenInformation,
            int tokeInfoLength,
            ref int reqLength
        );

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool LoadUserProfile(IntPtr token, ref Profileinfo profileInfo);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr processHandle,
            UInt32 desiredAccess, out IntPtr tokenHandle);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool UnloadUserProfile(IntPtr token, IntPtr profile);

        [StructLayout(LayoutKind.Sequential)]
        public struct SidAndAttributes
        {
            public IntPtr Sid;
            public int Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Profileinfo
        {
            public int Size;
            public int Flags;

            [MarshalAs(UnmanagedType.LPTStr)]
            public string UserName;

            [MarshalAs(UnmanagedType.LPTStr)] private readonly string ProfilePath;

            [MarshalAs(UnmanagedType.LPTStr)] private readonly string DefaultPath;

            [MarshalAs(UnmanagedType.LPTStr)] private readonly string ServerName;

            [MarshalAs(UnmanagedType.LPTStr)] private readonly string PolicyPath;

            public readonly IntPtr Profile;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TokenUser
        {
            public SidAndAttributes User;
        }

        #endregion Native Calls (P/Invoke and data structures)
    }
}
