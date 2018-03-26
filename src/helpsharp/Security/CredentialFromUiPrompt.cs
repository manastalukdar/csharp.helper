using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace helpsharp.Security
{
    public class CredentialFromUiPrompt
    {
        #region Private Enums

        private enum PromptForWindowsCredentialsFlags
        {
            /// <summary>
            /// The caller is requesting that the credential provider return the user name and password in plain text.
            /// This value cannot be combined with SECURE_PROMPT.
            /// </summary>
            CreduiwinGeneric = 0x1,

            /// <summary>
            /// The Save check box is displayed in the dialog box.
            /// </summary>
            CreduiwinCheckbox = 0x2,

            /// <summary>
            /// Only credential providers that support the authentication package specified by the authPackage parameter should be enumerated.
            /// This value cannot be combined with CREDUIWIN_IN_CRED_ONLY.
            /// </summary>
            CreduiwinAuthpackageOnly = 0x10,

            /// <summary>
            /// Only the credentials specified by the InAuthBuffer parameter for the authentication package specified by the authPackage parameter should be enumerated.
            /// If this flag is set, and the InAuthBuffer parameter is NULL, the function fails.
            /// This value cannot be combined with CREDUIWIN_AUTHPACKAGE_ONLY.
            /// </summary>
            CreduiwinInCredOnly = 0x20,

            /// <summary>
            /// Credential providers should enumerate only administrators. This value is intended for User Account Control (UAC) purposes only. We recommend that external callers not set this flag.
            /// </summary>
            CreduiwinEnumerateAdmins = 0x100,

            /// <summary>
            /// Only the incoming credentials for the authentication package specified by the authPackage parameter should be enumerated.
            /// </summary>
            CreduiwinEnumerateCurrentUser = 0x200,

            /// <summary>
            /// The credential dialog box should be displayed on the secure desktop. This value cannot be combined with CREDUIWIN_GENERIC.
            /// Windows Vista: This value is not supported until Windows Vista with SP1.
            /// </summary>
            CreduiwinSecurePrompt = 0x1000,

            /// <summary>
            /// The credential provider should align the credential BLOB pointed to by the refOutAuthBuffer parameter to a 32-bit boundary, even if the provider is running on a 64-bit system.
            /// </summary>
            CreduiwinPack32Wow = 0x10000000,
        }

        #endregion Private Enums

        #region Public Methods

        public long GetCredential(string captionText, string messageText, IntPtr parentHandle, out NetworkCredential networkCredential)
        {
            var save = false;
            var errorcode = 0;
            uint dialogReturn;
            uint authPackage = 0;
            IntPtr outCredBuffer;
            uint outCredSize;

            var credui = new CreduiInfo();
            credui.Size = Marshal.SizeOf(credui);
            credui.PszCaptionText = captionText;
            credui.PszMessageText = messageText;
            credui.HwndParent = parentHandle;

            while (true) // Show the dialog again and again, until Cancel is clicked or the entered credentials are correct.
            {
                // Show the dialog
                dialogReturn = CredUIPromptForWindowsCredentials(ref credui,
                errorcode,
                ref authPackage,
                IntPtr.Zero,  // A specific username can be forced to be shown in the dialog from here. Create it with 'CredPackAuthenticationBuffer()'. Then, the buffer goes here...
                0,          // ...and the size goes here. You also have to add CREDUIWIN_IN_CRED_ONLY to the flags (last argument).
                out outCredBuffer,
                out outCredSize,
                ref save,
                PromptForWindowsCredentialsFlags.CreduiwinGeneric); // Use the PromptForWindowsCredentialsFlags-Enum here. You can use multiple flags if you seperate them with | .

                if (dialogReturn != 0) // cancel returns 1223
                {
                    errorcode = (int)dialogReturn;
                    break; // Break, if Cancel was clicked or an error occurred
                }

                var usernameBuf = new StringBuilder(100);
                var passwordBuf = new StringBuilder(100);
                var domainBuf = new StringBuilder(100);

                var maxUserName = 100;
                var maxDomain = 100;
                var maxPassword = 100;

                var credentialEnteredCorrect = false;

                if (dialogReturn == 0)
                {
                    credentialEnteredCorrect = CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName, domainBuf, ref maxDomain, passwordBuf, ref maxPassword);
                    if (credentialEnteredCorrect)
                    {
                        // clear the memory allocated by CredUIPromptForWindowsCredentials
                        CoTaskMemFree(outCredBuffer);
                        networkCredential = new NetworkCredential()
                        {
                            UserName = usernameBuf.ToString(),
                            Password = passwordBuf.ToString(),
                            Domain = domainBuf.ToString()
                        };
                        return errorcode;
                    }
                    else
                    {
                        errorcode = 1326;
                    }

                    networkCredential = null;
                }
            }

            networkCredential = null;
            return errorcode;
        }

        #endregion Public Methods

        #region Private Methods

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr pv);

        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        private static extern uint CredUIPromptForWindowsCredentials(ref CreduiInfo notUsedHere,
          int authError,
          ref uint authPackage,
          IntPtr inAuthBuffer,
          uint inAuthBufferSize,
          out IntPtr refOutAuthBuffer,
          out uint refOutAuthBufferSize,
          ref bool save,
          PromptForWindowsCredentialsFlags flags);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern bool CredUnPackAuthenticationBuffer(int flags,
            IntPtr ptrauthBuffer,
            uint authBuffer,
            StringBuilder userName,
            ref int maxUserName,
            StringBuilder domainName,
            ref int maxDomainame,
            StringBuilder password,
            ref int maxPassword);

        #endregion Private Methods

        #region Private Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CreduiInfo
        {
            public int Size;
            public IntPtr HwndParent;
            public string PszMessageText;
            public string PszCaptionText;
            public IntPtr HbmBanner;
        }

        #endregion Private Structs
    }
}