using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace csharp.helper.Security
{
    public class LocalSecurityAuthority
    {
        #region enum all policies

        [Flags]
        private enum LsaAccessPolicy : long
        {
            PolicyViewLocalInformation = 0x00000001L,
            PolicyViewAuditInformation = 0x00000002L,
            PolicyGetPrivateInformation = 0x00000004L,
            PolicyTrustAdmin = 0x00000008L,
            PolicyCreateAccount = 0x00000010L,
            PolicyCreateSecret = 0x00000020L,
            PolicyCreatePrivilege = 0x00000040L,
            PolicySetDefaultQuotaLimits = 0x00000080L,
            PolicySetAuditRequirements = 0x00000100L,
            PolicyAuditLogAdmin = 0x00000200L,
            PolicyServerAdmin = 0x00000400L,
            PolicyLookupNames = 0x00000800L,
            PolicyNotification = 0x00001000L
        }

        #endregion enum all policies

        #region Public Methods

        public bool CheckRight(string accountName, string privilegeName)
        {
            accountName = GetSanitizedAccountName(accountName);

            // contains the last error
            long winErrorCode = 0;

            // pointer an size for the SID
            var sid = IntPtr.Zero;
            var sidSize = 0;

            // StringBuilder and size for the domain name
            var domainName = new StringBuilder();
            var nameSize = 0;

            // account-type variable for lookup
            var accountType = 0;

            // get required buffer size
            LookupAccountName(string.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

            // allocate buffers
            domainName = new StringBuilder(nameSize);
            sid = Marshal.AllocHGlobal(sidSize);

            // lookup the SID for the account
            var result = LookupAccountName(string.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

            // log info
            ////Console.WriteLine("LookupAccountName result = " + result);
            ////Console.WriteLine("IsValidSid: " + IsValidSid(sid));
            ////Console.WriteLine("LookupAccountName domainName: " + domainName.ToString());

            if (!result)
            {
                winErrorCode = GetLastError();
                throw new Exception("LookupAccountName failed.  Win32 Error Code: " +
                                                   Marshal.GetLastWin32Error() + "|| Message: " +
                     new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }

            // initialize an empty unicode-string
            var systemName = new LsaUnicodeString();

            // combine all policies
            const uint access = (uint)(
                 LsaAccessPolicy.PolicyAuditLogAdmin |
                 LsaAccessPolicy.PolicyCreateAccount |
                 LsaAccessPolicy.PolicyCreatePrivilege |
                 LsaAccessPolicy.PolicyCreateSecret |
                 LsaAccessPolicy.PolicyGetPrivateInformation |
                 LsaAccessPolicy.PolicyLookupNames |
                 LsaAccessPolicy.PolicyNotification |
                 LsaAccessPolicy.PolicyServerAdmin |
                 LsaAccessPolicy.PolicySetAuditRequirements |
                 LsaAccessPolicy.PolicySetDefaultQuotaLimits |
                 LsaAccessPolicy.PolicyTrustAdmin |
                 LsaAccessPolicy.PolicyViewAuditInformation |
                 LsaAccessPolicy.PolicyViewLocalInformation);

            // initialize a pointer for the policy handle
            IntPtr policyHandle;

            // these attributes are not used, but LsaOpenPolicy wants them to exists
            var objectAttributes = new LsaObjectAttributes();
            objectAttributes.Length = 0;
            objectAttributes.RootDirectory = IntPtr.Zero;
            objectAttributes.Attributes = 0;
            objectAttributes.SecurityDescriptor = IntPtr.Zero;
            objectAttributes.SecurityQualityOfService = IntPtr.Zero;

            // get a policy handle
            var resultPolicy = LsaOpenPolicy(ref systemName, ref objectAttributes, access, out policyHandle);
            winErrorCode = LsaNtStatusToWinError(resultPolicy);

            if (winErrorCode != 0)
            {
                var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                throw new Exception("OpenPolicy failed. Error code: " + winErrorCode + "|| ErrorMessage: " + errorMessage);
            }
            else
            {
                var rightsArray = IntPtr.Zero;
                ulong rightsCount = 0;
                LsaEnumerateAccountRights(policyHandle, sid, out rightsArray, out rightsCount);
                winErrorCode = LsaNtStatusToWinError(resultPolicy);

                if (winErrorCode != 0)
                {
                    var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                    throw new Exception("EnumerateAccountRights failed. Error code: " + winErrorCode + "|| ErrorMessage: " + errorMessage);
                }
                else
                {
                    var myLsaus = new LsaUnicodeString();
                    for (ulong i = 0; i < rightsCount; i++)
                    {
                        var itemAddr = new IntPtr(rightsArray.ToInt64() + (long)(i * (ulong)Marshal.SizeOf(myLsaus)));
                        myLsaus = (LsaUnicodeString)Marshal.PtrToStructure(itemAddr, myLsaus.GetType());
                        var thisRight = Lsaus2String(myLsaus);

                        if (string.Compare(thisRight, privilegeName, StringComparison.OrdinalIgnoreCase) != 0) continue;
                        LsaClose(policyHandle);
                        FreeSid(sid);
                        return true;
                    }
                }

                LsaClose(policyHandle);
            }

            FreeSid(sid);
            return false;
        }

        public long RemoveRight(string accountName, string privilegeName)
        {
            accountName = GetSanitizedAccountName(accountName);

            // contains the last error
            long winErrorCode = 0;

            // pointer an size for the SID
            var sid = IntPtr.Zero;
            var sidSize = 0;

            // StringBuilder and size for the domain name
            var domainName = new StringBuilder();
            var nameSize = 0;

            // account-type variable for lookup
            var accountType = 0;

            // get required buffer size
            LookupAccountName(string.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

            // allocate buffers
            domainName = new StringBuilder(nameSize);
            sid = Marshal.AllocHGlobal(sidSize);

            // lookup the SID for the account
            var result = LookupAccountName(string.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

            // log info
            ////Console.WriteLine("LookupAccountName result = " + result);
            ////Console.WriteLine("IsValidSid: " + IsValidSid(sid));
            ////Console.WriteLine("LookupAccountName domainName: " + domainName.ToString());

            if (!result)
            {
                winErrorCode = GetLastError();
                throw new Exception("LookupAccountName failed: " + winErrorCode);
            }
            else
            {
                // initialize an empty unicode-string
                var systemName = new LsaUnicodeString();

                // combine all policies
                var access = (uint)(
                     LsaAccessPolicy.PolicyAuditLogAdmin |
                     LsaAccessPolicy.PolicyCreateAccount |
                     LsaAccessPolicy.PolicyCreatePrivilege |
                     LsaAccessPolicy.PolicyCreateSecret |
                     LsaAccessPolicy.PolicyGetPrivateInformation |
                     LsaAccessPolicy.PolicyLookupNames |
                     LsaAccessPolicy.PolicyNotification |
                     LsaAccessPolicy.PolicyServerAdmin |
                     LsaAccessPolicy.PolicySetAuditRequirements |
                     LsaAccessPolicy.PolicySetDefaultQuotaLimits |
                     LsaAccessPolicy.PolicyTrustAdmin |
                     LsaAccessPolicy.PolicyViewAuditInformation |
                     LsaAccessPolicy.PolicyViewLocalInformation);

                // initialize a pointer for the policy handle

                // these attributes are not used, but LsaOpenPolicy wants them to exists
                var objectAttributes = new LsaObjectAttributes();
                objectAttributes.Length = 0;
                objectAttributes.RootDirectory = IntPtr.Zero;
                objectAttributes.Attributes = 0;
                objectAttributes.SecurityDescriptor = IntPtr.Zero;
                objectAttributes.SecurityQualityOfService = IntPtr.Zero;

                // get a policy handle
                var resultPolicy = LsaOpenPolicy(ref systemName, ref objectAttributes, access, out IntPtr policyHandle);
                winErrorCode = LsaNtStatusToWinError(resultPolicy);

                if (winErrorCode != 0)
                {
                    var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                    throw new Exception("OpenPolicy failed: " + winErrorCode + " ErrorMessage: " + errorMessage);
                }
                else
                {
                    // Now that we have the SID an the policy, we can add rights to the account.

                    // initialize an unicode-string for the privilege name
                    var userRights = new LsaUnicodeString[1];
                    userRights[0] = new LsaUnicodeString();
                    userRights[0].Buffer = Marshal.StringToHGlobalUni(privilegeName);
                    userRights[0].Length = (ushort)(privilegeName.Length * UnicodeEncoding.CharSize);
                    userRights[0].MaximumLength = (ushort)((privilegeName.Length + 1) * UnicodeEncoding.CharSize);

                    // add the right to the account
                    var res = LsaRemoveAccountRights(policyHandle, sid, false, userRights, 1);
                    winErrorCode = LsaNtStatusToWinError(res);
                    if (winErrorCode != 0)
                    {
                        var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                        throw new Exception("LsaRemoveAccountRights failed: " + winErrorCode + " Error Message: " + errorMessage);
                    }

                    LsaClose(policyHandle);
                }

                FreeSid(sid);
            }

            return winErrorCode;
        }

        /// <summary>
        /// Adds a privilege to an account
        /// </summary>
        /// <param name="accountName">Name of an account - "domain\account" or only "account"</param>
        /// <param name="privilegeName">Name ofthe privilege</param>
        /// <returns>The windows error code returned by LsaAddAccountRights</returns>
        public long SetRight(string accountName, string privilegeName)
        {
            accountName = GetSanitizedAccountName(accountName);

            // contains the last error
            long winErrorCode = 0;

            // pointer an size for the SID
            var sid = IntPtr.Zero;
            var sidSize = 0;

            // StringBuilder and size for the domain name
            var domainName = new StringBuilder();
            var nameSize = 0;

            // account-type variable for lookup
            var accountType = 0;

            // get required buffer size
            LookupAccountName(string.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

            // allocate buffers
            domainName = new StringBuilder(nameSize);
            sid = Marshal.AllocHGlobal(sidSize);

            // lookup the SID for the account
            var result = LookupAccountName(string.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

            // log info
            ////Console.WriteLine("LookupAccountName result = " + result);
            ////Console.WriteLine("IsValidSid: " + IsValidSid(sid));
            ////Console.WriteLine("LookupAccountName domainName: " + domainName.ToString());

            if (!result)
            {
                winErrorCode = GetLastError();
                throw new Exception("LookupAccountName failed: " + winErrorCode);
            }
            // initialize an empty unicode-string
            var systemName = new LsaUnicodeString();

            // combine all policies
            const uint access = (uint)(
                LsaAccessPolicy.PolicyAuditLogAdmin |
                LsaAccessPolicy.PolicyCreateAccount |
                LsaAccessPolicy.PolicyCreatePrivilege |
                LsaAccessPolicy.PolicyCreateSecret |
                LsaAccessPolicy.PolicyGetPrivateInformation |
                LsaAccessPolicy.PolicyLookupNames |
                LsaAccessPolicy.PolicyNotification |
                LsaAccessPolicy.PolicyServerAdmin |
                LsaAccessPolicy.PolicySetAuditRequirements |
                LsaAccessPolicy.PolicySetDefaultQuotaLimits |
                LsaAccessPolicy.PolicyTrustAdmin |
                LsaAccessPolicy.PolicyViewAuditInformation |
                LsaAccessPolicy.PolicyViewLocalInformation);

            // initialize a pointer for the policy handle
            var policyHandle = IntPtr.Zero;

            // these attributes are not used, but LsaOpenPolicy wants them to exists
            var objectAttributes = new LsaObjectAttributes();
            objectAttributes.Length = 0;
            objectAttributes.RootDirectory = IntPtr.Zero;
            objectAttributes.Attributes = 0;
            objectAttributes.SecurityDescriptor = IntPtr.Zero;
            objectAttributes.SecurityQualityOfService = IntPtr.Zero;

            // get a policy handle
            var resultPolicy = LsaOpenPolicy(ref systemName, ref objectAttributes, access, out policyHandle);
            winErrorCode = LsaNtStatusToWinError(resultPolicy);

            if (winErrorCode != 0)
            {
                var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                throw new Exception("OpenPolicy failed: " + winErrorCode + " ErrorMessage: " + errorMessage);
            }
            else
            {
                // Now that we have the SID an the policy, we can add rights to the account.

                // initialize an unicode-string for the privilege name
                var userRights = new LsaUnicodeString[1];
                userRights[0] = new LsaUnicodeString();
                userRights[0].Buffer = Marshal.StringToHGlobalUni(privilegeName);
                userRights[0].Length = (ushort)(privilegeName.Length * UnicodeEncoding.CharSize);
                userRights[0].MaximumLength = (ushort)((privilegeName.Length + 1) * UnicodeEncoding.CharSize);

                // add the right to the account
                var res = LsaAddAccountRights(policyHandle, sid, userRights, 1);
                winErrorCode = LsaNtStatusToWinError(res);
                if (winErrorCode != 0)
                {
                    var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                    throw new Exception("LsaAddAccountRights failed: " + winErrorCode + " Error Message: " + errorMessage);
                }

                LsaClose(policyHandle);
            }

            FreeSid(sid);

            return winErrorCode;
        }

        private static string Lsaus2String(LsaUnicodeString lsaus)
        {
            var cvt = new char[lsaus.Length / UnicodeEncoding.CharSize];
            Marshal.Copy(lsaus.Buffer, cvt, 0, lsaus.Length / UnicodeEncoding.CharSize);
            return new string(cvt);
        }

        private static LsaUnicodeString String2Lsaus(string myString)
        {
            var retStr = new LsaUnicodeString();
            retStr.Buffer = Marshal.StringToHGlobalUni(myString);
            retStr.Length = (ushort)(myString.Length * UnicodeEncoding.CharSize);
            retStr.MaximumLength = (ushort)((myString.Length + 1) * UnicodeEncoding.CharSize);
            return retStr;
        }

        #endregion Public Methods

        #region Private Methods

        private string GetSanitizedAccountName(string accountName)
        {
            // if local account has been passed as ".\account" remove the ".\"
            if (accountName.IndexOf("\\", StringComparison.Ordinal) != -1)
            {
                var index = accountName.LastIndexOf("\\", StringComparison.Ordinal);
                var domain = accountName.Substring(0, index);
                if (string.Compare(domain, ".") == 0)
                {
                    accountName = accountName.Replace(domain + "\\", string.Empty);
                }
            }

            return accountName;
        }

        #endregion Private Methods

        #region Import LSA functions

        [DllImport("advapi32")]
        private static extern void FreeSid(IntPtr sid);

        [DllImport("kernel32.dll")]
        private static extern int GetLastError();

        [DllImport("advapi32.dll")]
        private static extern bool IsValidSid(IntPtr sid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, PreserveSig = true)]
        private static extern bool LookupAccountName(
             string systemName, string accountName,
             IntPtr psid,
             ref int cbsid,
             StringBuilder domainName, ref int cbdomainLength, ref int use);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaAddAccountRights(
             IntPtr policyHandle,
             IntPtr accountSid,
             LsaUnicodeString[] userRights,
             uint countOfRights);

        [DllImport("advapi32.dll")]
        private static extern long LsaClose(IntPtr objectHandle);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern long LsaEnumerateAccountRights(
            IntPtr policyHandle, IntPtr accountSid,
            out /* LSA_UNICODE_STRING[] */ IntPtr userRights,
            out ulong countOfRights);

        [DllImport("advapi32.dll")]
        private static extern uint LsaNtStatusToWinError(uint status);

        [DllImport("advapi32.dll", PreserveSig = true)]
        private static extern uint LsaOpenPolicy(
             ref LsaUnicodeString systemName,
             ref LsaObjectAttributes objectAttributes,
             uint desiredAccess,
             out IntPtr policyHandle);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaRemoveAccountRights(
            IntPtr policyHandle,
            IntPtr accountSid,
            [MarshalAs(UnmanagedType.U1)]
            bool allRights,
            LsaUnicodeString[] userRights,
            uint countOfRights);

        #endregion Import LSA functions

        #region define structures

        [StructLayout(LayoutKind.Sequential)]
        private struct LsaObjectAttributes
        {
            public int Length;
            public IntPtr RootDirectory;
            private readonly LsaUnicodeString ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LsaUnicodeString
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        #endregion define structures
    }
}