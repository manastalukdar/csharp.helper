using System;
using System.Security;

namespace helpsharp.Security.CredentialManagement
{
    /// <summary>
    ///     Credentials.
    /// </summary>
    public class Credentials : IDisposable
    {
        #region Public Constructors

        public Credentials(string key, SecureString userName, SecureString password)
        {
            Key = key;
            UserName = userName;
            Password = password;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        ///     Key used to access the credentials in the Windows Credential Manager's store.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        ///     Password.
        /// </summary>
        public SecureString Password { get; }

        /// <summary>
        ///     User name.
        /// </summary>
        public SecureString UserName { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            // Clear the password.
            Password?.Clear();
        }

        #endregion Protected Methods
    }
}