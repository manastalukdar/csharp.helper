using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace csharp.helper.Security
{
    /// ------------------------------------------------------------------------------------------------
    /// <summary>
    /// SecureString conversions extensions. For more details, please see: http:
    /// //blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-
    /// string.aspx.
    /// </summary>
    /// ------------------------------------------------------------------------------------------------
    public static class SecureStringExtensions
    {
        #region Secure string handling.

        /// ------------------------------------------------------------------------------------------------
        /// <summary>Convert a string to a SecureString.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="unsecureString">Unsecure string to convert.</param>
        /// <returns>SecureString returned.</returns>
        /// ------------------------------------------------------------------------------------------------
        public static SecureString ConvertToSecureString(this string unsecureString)
        {
            // Sanity check...
            if (unsecureString == null)
            {
                throw new ArgumentNullException(nameof(unsecureString));
            }

            // Create a SecureString and copy the contents of the string to the SecureString.
            var secureString = new SecureString();
            foreach (var c in unsecureString)
            {
                secureString.AppendChar(c);
            }

            // Make the SecureString read-only.
            secureString.MakeReadOnly();

            // Return it.
            return secureString;
        }

        /// ------------------------------------------------------------------------------------------------
        /// <summary>Convert a SecureString to a string.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="secureString">SecureString to convert.</param>
        /// <returns>string returned.</returns>
        /// ------------------------------------------------------------------------------------------------
        public static string ConvertToUnsecureString(this SecureString secureString)
        {
            // Sanity check...
            if (secureString == null)
            {
                throw new ArgumentNullException(nameof(secureString));
            }

            var unmanagedString = IntPtr.Zero;
            try
            {
                // Get the unmanaged string corresponding to the SecureString.
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);

                // Create a managed string from it.
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                // Free the unamanaged memory so that we don't leak the secure string.
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        #endregion Secure string handling.
    }
}