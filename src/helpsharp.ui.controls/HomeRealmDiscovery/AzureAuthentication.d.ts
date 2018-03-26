declare module server {
	interface azureAuthentication {
		windowsIdentityToImpersonate: {
			authenticationType: string;
			impersonationLevel: any;
			isAuthenticated: boolean;
			isGuest: boolean;
			isSystem: boolean;
			isAnonymous: boolean;
			name: string;
			owner: {
				binaryLength: number;
				accountDomainSid: any;
				value: string;
			};
			user: {
				binaryLength: number;
				accountDomainSid: any;
				value: string;
			};
			groups: {
				count: number;
				isReadOnly: boolean;
				this: {
					value: string;
				};
			};
			token: any;
			accessToken: {
				isInvalid: boolean;
			};
			userClaims: any[];
			deviceClaims: any[];
			claims: any[];
		};
	}
}
