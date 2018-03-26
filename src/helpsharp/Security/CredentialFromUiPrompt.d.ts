declare module server {
    const enum promptForWindowsCredentialsFlags {
        /** The caller is requesting that the credential provider return the user name and password in plain text.This value cannot be combined with SECURE_PROMPT. */
        cREDUIWIN_GENERIC = 0x1,
        /** The Save check box is displayed in the dialog box. */
        cREDUIWIN_CHECKBOX = 0x2,
        /** Only credential providers that support the authentication package specified by the authPackage parameter should be enumerated.This value cannot be combined with CREDUIWIN_IN_CRED_ONLY. */
        cREDUIWIN_AUTHPACKAGE_ONLY = 0x10,
        /** Only the credentials specified by the InAuthBuffer parameter for the authentication package specified by the authPackage parameter should be enumerated.If this flag is set, and the InAuthBuffer parameter is NULL, the function fails.This value cannot be combined with CREDUIWIN_AUTHPACKAGE_ONLY. */
        cREDUIWIN_IN_CRED_ONLY = 0x20,
        /** Credential providers should enumerate only administrators. This value is intended for User Account Control (UAC) purposes only. We recommend that external callers not set this flag. */
        cREDUIWIN_ENUMERATE_ADMINS = 0x100,
        /** Only the incoming credentials for the authentication package specified by the authPackage parameter should be enumerated. */
        cREDUIWIN_ENUMERATE_CURRENT_USER = 0x200,
        /** The credential dialog box should be displayed on the secure desktop. This value cannot be combined with CREDUIWIN_GENERIC.Windows Vista: This value is not supported until Windows Vista with SP1. */
        cREDUIWIN_SECURE_PROMPT = 0x1000,
        /** The credential provider should align the credential BLOB pointed to by the refOutAuthBuffer parameter to a 32-bit boundary, even if the provider is running on a 64-bit system. */
        cREDUIWIN_PACK_32_WOW = 0x10000000,
    }
    interface credentialFromUiPrompt {
    }
}