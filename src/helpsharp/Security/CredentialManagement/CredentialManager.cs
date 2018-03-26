using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace helpsharp.Security.CredentialManagement
{
    /// <summary>
    ///     Credential Manager is a wrapper around the Windows Credential Manager's unmanaged API. For more details, please
    ///     see:
    ///     http://blogs.msdn.com/b/peerchan/archive/2005/11/01/487834.aspx
    ///     http://msdn.microsoft.com/en-us/library/windows/desktop/aa374788(v=vs.85).aspx
    /// </summary>
    public static class CredentialManager
    {
        #region Private Fields

        /// <summary>
        ///     Maximum size of password.
        /// </summary>
        private const int MaximumCredentialBlobSize = 512;

        #endregion Private Fields

        #region Public Enums

        /// <summary>
        ///     Credential Type.
        /// </summary>
        public enum CredentialType
        {
            /// <summary>
            ///     The credential is a generic credential. The credential will not be used by any particular authentication package.
            ///     The credential will be stored securely but has no other significant characteristics.
            /// </summary>
            CredTypeGeneric = 0x1,

            /// <summary>
            ///     The credential is a password credential and is specific to Microsoft's authentication packages. The NTLM, Kerberos,
            ///     and Negotiate authentication packages will automatically use this credential when connecting to the named target.
            /// </summary>
            CredTypeDomainPassword = 0x2,

            /// <summary>
            ///     The credential is a certificate credential and is specific to Microsoft's authentication packages. The Kerberos,
            ///     Negotiate,
            ///     and Schannel authentication packages automatically use this credential when connecting to the named target.
            /// </summary>
            CredTypeDomainCertificate = 0x3
        }

        /// <summary>
        ///     Persistance type.
        /// </summary>
        public enum Persistance
        {
            /// <summary>
            ///     The credential persists for the life of the logon session. It will not be visible to other logon sessions of this
            ///     same user.
            ///     It will not exist after this user logs off and back on.
            /// </summary>
            CredPersistSession = 0x1,

            /// <summary>
            ///     The credential persists for all subsequent logon sessions on this same computer. It is visible to other logon
            ///     sessions of this
            ///     same user on this same computer and not visible to logon sessions for this user on other computers.
            /// </summary>
            CredPersistLocalMachine = 0x2,

            /// <summary>
            ///     The credential persists for all subsequent logon sessions on this same computer. It is visible to other logon
            ///     sessions of this same
            ///     user on this same computer and to logon sessions for this user on other computers. This option can be implemented
            ///     as locally persisted
            ///     credential if the administrator or user configures the user account to not have roam-able state. For instance, if
            ///     the user has no roaming
            ///     profile, the credential will only persist locally.
            /// </summary>
            CredPersistEnterprise = 0x3
        }

        #endregion Public Enums

        #region Public Methods

        /// <summary>
        ///     Delete the credentials associated with <paramref name="key" /> from the Windows Credential Manager's store.
        /// </summary>
        /// <param name="key">
        ///     Key.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>
        ///     Windows status code - 0 is success.
        /// </returns>
        public static int DeleteCredentials(string key)
        {
            // Sanity check...
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            // Make the API call using the P/Invoke signature.
            CredDelete(key, CredentialType.CredTypeGeneric, 0);
            return Marshal.GetLastWin32Error();
        }

        /// <summary>
        ///     Read the credentials corresponding to <paramref name="key" /> from the Windows Credential Manager's store.
        /// </summary>
        /// <param name="key">Key to the credentials to be read.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns>
        ///     Credentials if they were found, null if they weren't.
        /// </returns>
        public static Credentials GetCredentials(string key)
        {
            // Sanity check...
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key is null or empty");

            // Read the credentials corresponding to "key" from the Windows Credential Manager's store.

            var readReturnCode = ReadCredentials(key, out SecureString userName, out SecureString password);
            if (readReturnCode == 0)
                return new Credentials(key, userName, password);

            Console.WriteLine("ReadCredentials Failed. Return code :: {0}", readReturnCode);
            throw new Exception("Return Code: " + readReturnCode + "|| Win32 Error Code: " + Marshal.GetLastWin32Error() +
                                "|| Message: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);

            // Return the credentials we just read.
        }

        /// <summary>
        ///     Read the credentials associated with <paramref name="key" /> from the Windows Credential Manager's store.
        /// </summary>
        /// <param name="key">
        ///     Key.
        /// </param>
        /// <param name="userName">
        ///     User name returned.
        /// </param>
        /// <param name="password">
        ///     Password returned.
        /// </param>
        /// <returns>
        ///     Windows status code - 0 is success.
        /// </returns>
        public static int ReadCredentials(string key, out SecureString userName, out SecureString password)
        {
            // Sanity check...
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key is null or empty");

            IntPtr credentialPointer;
            userName = null;
            password = null;

            // Make the API call using the P/Invoke signature.
            var read = CredRead(key, CredentialType.CredTypeGeneric, 0, out credentialPointer);
            var lastError = Marshal.GetLastWin32Error();

            if (!read)
                return lastError;

            // If the API was successful then extract the credential.
            using (var critCred = new CriticalCredentialHandle(credentialPointer))
            {
                var cred = critCred.GetCredential();

                var passwordBytes = new byte[cred.CredentialBlobSize];

                // Copy the memory from the blob to our array
                Marshal.Copy(cred.CredentialBlob, passwordBytes, 0, (int)cred.CredentialBlobSize);

                userName = cred.UserName == IntPtr.Zero
                    ? new SecureString()
                    : Marshal.PtrToStringUni(cred.UserName).ConvertToSecureString();
                password = Encoding.Unicode.GetString(passwordBytes).ConvertToSecureString();
            }

            return 0;
        }

        /// <summary>
        ///     Write the credentials to the Windows Credential Manager's store.
        /// </summary>
        /// <param name="key">
        ///     Key.
        /// </param>
        /// <param name="userName">
        ///     User name.
        /// </param>
        /// <param name="password">
        ///     Password.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>
        ///     Windows status code - 0 is success.
        /// </returns>
        public static int WriteCredentials(string key, SecureString userName, SecureString password)
        {
            // Sanity check...
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key is null or empty");

            if (userName == null || userName.Length == 0)
                throw new ArgumentNullException("userName is null or empty");

            if (password == null || password.Length == 0)
                throw new ArgumentNullException("password is null or empty");

            var passwordAsString = password.ConvertToUnsecureString();
            var byteArray = Encoding.Unicode.GetBytes(passwordAsString);

            if (byteArray.Length > MaximumCredentialBlobSize)
                throw new ArgumentOutOfRangeException(string.Format("The password message has exceeded {0} bytes.",
                    MaximumCredentialBlobSize));

            // Setup the Credential object.
            var cred = new Credential
            {
                TargetName = Marshal.StringToCoTaskMemUni(key),
                CredentialBlob = Marshal.StringToCoTaskMemUni(passwordAsString),
                CredentialBlobSize = (uint)Encoding.Unicode.GetBytes(passwordAsString).Length,
                AttributeCount = 0,
                Attributes = IntPtr.Zero,
                Comment = IntPtr.Zero,
                TargetAlias = Marshal.StringToCoTaskMemUni(key),
                Type = CredentialType.CredTypeGeneric,
                Persist = Persistance.CredPersistEnterprise,
                UserName =
                    Marshal.StringToCoTaskMemUni(userName.ConvertToUnsecureString())
            };

            // Make the API call using the P/Invoke signature.
            var written = CredWrite(ref cred, 0);
            var lastError = Marshal.GetLastWin32Error();

            return !written ? lastError : 0;
        }

        #endregion Public Methods

        #region Private Methods

        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredDelete(string target, CredentialType type, int reservedFlag);

        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CredentialType type, int reservedFlag,
            out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredWrite([In] ref Credential userCredential, [In] uint flags);

        #endregion Private Methods

        #region Private Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct Credential
        {
            private readonly uint Flags;
            public CredentialType Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            private readonly FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public Persistance Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NativeCredential
        {
            private readonly uint Flags;
            private uint Type;
            private IntPtr TargetName;
            private IntPtr Comment;
            private readonly FILETIME LastWritten;
            private uint CredentialBlobSize;
            private IntPtr CredentialBlob;
            private uint Persist;
            private uint AttributeCount;
            private IntPtr Attributes;
            private IntPtr TargetAlias;
            private IntPtr UserName;

            /// <summary>
            ///     This method derives a NativeCredential instance from a given Credential instance.
            /// </summary>
            /// <param name="cred">
            ///     The managed Credential counterpart containing data to be stored.
            /// </param>
            /// <returns>
            ///     A NativeCredential instance that is derived from the given Credential instance.
            /// </returns>
            internal static NativeCredential GetNativeCredential(Credential cred, string target, string passwordAsString)
            {
                var ncred = new NativeCredential
                {
                    AttributeCount = 0,
                    Attributes = IntPtr.Zero,
                    Comment = IntPtr.Zero,
                    TargetAlias = IntPtr.Zero,
                    Type = (uint)CredentialType.CredTypeGeneric,
                    Persist = (uint)Persistance.CredPersistSession,
                    CredentialBlobSize = (uint)Encoding.Unicode.GetBytes(passwordAsString).Length,
                    TargetName = Marshal.StringToCoTaskMemUni(target),
                    CredentialBlob = Marshal.StringToCoTaskMemUni(passwordAsString)
                };

                ncred.UserName = cred.UserName != IntPtr.Zero ? cred.UserName : Marshal.StringToCoTaskMemUni(Environment.UserName);

                return ncred;
            }
        }

        #endregion Private Structs

        #region Private Classes

        private sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
        {
            #region Internal Constructors

            /// <summary>
            ///     Set the handle.
            /// </summary>
            /// <param name="preexistingHandle">
            ///     Handle.
            /// </param>
            internal CriticalCredentialHandle(IntPtr preexistingHandle)
            {
                SetHandle(preexistingHandle);
            }

            #endregion Internal Constructors

            #region Internal Methods

            /// <summary>
            ///     Extract the Credential from the NativeCredential returned.
            /// </summary>
            /// <returns>
            ///     Credential object.
            /// </returns>
            internal Credential GetCredential()
            {
                if (IsInvalid) throw new InvalidOperationException("Invalid CriticalHandle!");
                // Get the Credential from the mem location
                return (Credential)Marshal.PtrToStructure(handle, typeof(Credential));
            }

            #endregion Internal Methods

            #region Protected Methods

            /// <summary>
            ///     Perform any specific actions to release the handle in the ReleaseHandle method.
            ///     Often, you need to use Pinvoke to make a call into the Win32 API to release the
            ///     handle. In this case, however, we can use the Marshal class to release the unmanaged memory.
            /// </summary>
            /// <returns>
            ///     Returns "true" if successful, "false" if it fails.
            /// </returns>
            protected override bool ReleaseHandle()
            {
                // If the handle was set, free it. Return success.
                if (IsInvalid) return false;
                // NOTE: We should also ZERO out the memory allocated to the handle, before free'ing it
                // so there are no traces of the sensitive data left in memory.
                CredFree(handle);

                // Mark the handle as invalid for future users.
                SetHandleAsInvalid();

                return true;

                // Return false.
            }

            #endregion Protected Methods
        }

        #endregion Private Classes
    }
}