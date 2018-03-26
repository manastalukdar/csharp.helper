//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Principal;
//using System.Windows.Controls;
//using System.Windows.Interop;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
//using helpsharp.Security;
//using csharp.ui.controls.HomeRealmDiscovery;

//namespace helpsharp.Azure
//{
//    public class AzureAuthentication
//    {
//        #region Private Fields

//        // Create an AAL AuthenticationContext object and link it to the tenant backing the
//        // Configuration service
//        private AuthenticationContext _authContext;

//        private AuthenticationResult _authenticationResult;
//        private Hrd _hrdPage;
//        private IdentityProviderDescriptor _selectedIdentityProviderDescriptor = null;
//        private string _selectedIdp = string.Empty;
//        private string _tenantServiceRealm;
//        private int _tokenCacheClockSkewInSecond = 3600;
//        ////private string _clientId = "1FB1165A-86F0-4CC6-86C2-F6BED656CB09";

//        private WindowsIdentity _windowsIdentityToImpersonate;

//        #endregion Private Fields

//        #region Public Constructors

//        public AzureAuthentication(string tenantRealm, string tenantAcsNamespaceUrl, int tokenCacheClockSkewInSecond)
//        {
//            // Create an AAL AuthenticationContext object and link it to the tenant backing the
//            // Configuration service
//            _authContext = new AuthenticationContext(tenantAcsNamespaceUrl);
//            _tenantServiceRealm = tenantRealm;
//            _tokenCacheClockSkewInSecond = tokenCacheClockSkewInSecond;
//        }

//        #endregion Public Constructors

//        #region Public Properties

//        public WindowsIdentity WindowsIdentityToImpersonate
//        {
//            get
//            {
//                return _windowsIdentityToImpersonate;
//            }

//            set
//            {
//                _windowsIdentityToImpersonate = value;
//            }
//        }

//        #endregion Public Properties

//        #region Public Methods

//        public AuthenticationResult AcquireToken(System.Windows.Forms.IWin32Window owner = null)
//        {
//            try
//            {
//                if (_selectedIdentityProviderDescriptor == null)
//                {
//                    List<IdentityProviderDescriptor> idps = new List<IdentityProviderDescriptor>();
//                    // Get the list of Idps
//                    if (WindowsIdentityToImpersonate != null)
//                    {
//                        Impersonation.Impersonate(WindowsIdentityToImpersonate, true);
//                        idps = (List<IdentityProviderDescriptor>)_authContext.GetProviders(_tenantServiceRealm);
//                        Impersonation.UnImpersonate();
//                    }
//                    else
//                    {
//                        idps = (List<IdentityProviderDescriptor>)_authContext.GetProviders(_tenantServiceRealm);
//                    }

//                    if (idps.Count > 1)
//                    {
//                        // pop up a Home Realm Discovery window and let the user choose an Idp
//                        ChooseIdp(idps, owner);
//                    }
//                    else
//                    {
//                        _selectedIdentityProviderDescriptor = idps[0];
//                    }
//                }

//                // It will use a pop-up window to initiate the logon flow.
//                if (owner != null)
//                {
//                    _authContext.OwnerWindow = owner;
//                }

//                _authenticationResult = _authContext.AcquireToken(_tenantServiceRealm, _selectedIdentityProviderDescriptor);

//                // credential will be null if the user hits cancel
//                if (_authenticationResult != null)
//                {
//                    return _authenticationResult;
//                }

//                throw new Exception("You must be authenticated to perform this operation.");
//            }
//            catch (ActiveDirectoryAuthenticationException ex)
//            {
//                string message = ex.Message;
//                if (ex.InnerException != null)
//                {
//                    message += " " + ex.InnerException.Message;
//                }

//                throw new Exception("AcquireToken ActiveDirectoryAuthenticationException: " + message);
//            }
//            catch (ArgumentException ex)
//            {
//                throw new Exception("AcquireToken Exception: " + ex.Message);
//            }
//        }

//        public bool IsTokenValid(AuthenticationResult authenticationResult)
//        {
//            if (authenticationResult == null || authenticationResult.AccessToken == null)
//            {
//                return false;
//            }

//            DateTime currentTime = DateTime.UtcNow;

//            // check expiry time
//            if (authenticationResult.ExpiresOn < currentTime.Subtract(TimeSpan.FromSeconds(_tokenCacheClockSkewInSecond)))
//            {
//                return false;
//            }

//            // token is valid
//            return true;
//        }

//        #endregion Public Methods

//        #region Private Methods

//        private void Button_Click(object sender, EventArgs e)
//        {
//            this._selectedIdp = (string)((Button)sender).Content;
//            this._hrdPage.Close();
//        }

//        private void ChooseIdp(List<IdentityProviderDescriptor> idps, System.Windows.Forms.IWin32Window owner = null)
//        {
//            List<Button> list = new List<Button>();

//            // create a Button for each Idp
//            foreach (IdentityProviderDescriptor idp in idps)
//            {
//                Button b = new Button();
//                b.Content = idp.Name;
//                b.Click += Button_Click;
//                list.Add(b);
//            }

//            _hrdPage = new Hrd();
//            WindowInteropHelper helper = new WindowInteropHelper(_hrdPage);

//            // Add the buttons to the Home Realm Discovery window
//            _hrdPage.AddButtons(list);

//            // pop up the Home Realm Discovery window
//            if (owner != null)
//            {
//                helper.Owner = owner.Handle;
//            }

//            _hrdPage.ShowInTaskbar = false;
//            _hrdPage.ShowActivated = true;
//            _hrdPage.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
//            _hrdPage.ShowDialog();

//            // select the Idp based on the Button clicked by the user
//            _selectedIdentityProviderDescriptor = idps.First(idp => idp.Name.Equals(this._selectedIdp));
//        }

//        #endregion Private Methods
//    }
//}