namespace helpsharp.Utility
{
    public class DefinitionsCommon
    {
        #region Public Enums

        public enum AccountRightsConstants
        {
            SeBatchLogonRight,
            SeDenyBatchLogonRight,
            SeDenyInteractiveLogonRight,
            SeDenyNetworkLogonRight,
            SeDenyRemoteInteractiveLogonRight,
            SeDenyServiceLogonRight,
            SeInteractiveLogonRight,
            SeNetworkLogonRight,
            SeRemoteInteractiveLogonRight,
            SeServiceLogonRight
        }

        #endregion Public Enums

        #region Public Methods

        public string GetAccountRightsConstantString(AccountRightsConstants accountRightsConstant)
        {
            if (accountRightsConstant == AccountRightsConstants.SeBatchLogonRight)
            {
                return "SeBatchLogonRight";
            }
            else if (accountRightsConstant == AccountRightsConstants.SeDenyBatchLogonRight)
            {
                return "SeDenyBatchLogonRight";
            }
            else if (accountRightsConstant == AccountRightsConstants.SeDenyInteractiveLogonRight)
            {
                return "SeDenyInteractiveLogonRight";
            }
            else if (accountRightsConstant == AccountRightsConstants.SeDenyRemoteInteractiveLogonRight)
            {
                return "SeDenyNetworkLogonRight";
            }
            else if (accountRightsConstant == AccountRightsConstants.SeDenyServiceLogonRight)
            {
                return "SeDenyServiceLogonRight";
            }
            else if (accountRightsConstant == AccountRightsConstants.SeInteractiveLogonRight)
            {
                return "SeInteractiveLogonRight";
            }
            else if (accountRightsConstant == AccountRightsConstants.SeNetworkLogonRight)
            {
                return "SeNetworkLogonRight";
            }
            else if (accountRightsConstant == AccountRightsConstants.SeRemoteInteractiveLogonRight)
            {
                return "SeRemoteInteractiveLogonRight";
            }
            else if (accountRightsConstant == AccountRightsConstants.SeServiceLogonRight)
            {
                return "SeServiceLogonRight";
            }
            else
            {
                return null;
            }
        }

        #endregion Public Methods
    }
}