import { useMsal, useAccount } from '@azure/msal-react';
import { loginRequest, apiScopes } from '../auth/msalConfig';

export function useAuth() {
  const { instance, accounts } = useMsal();
  const account = useAccount(accounts[0] ?? {});

  const login = () => instance.loginRedirect(loginRequest);
  const logout = () => instance.logoutRedirect();

  const getAccessToken = async (): Promise<string> => {
    if (!account) throw new Error('No account');
    const response = await instance.acquireTokenSilent({
      ...apiScopes,
      account,
    });
    return response.accessToken;
  };

  return {
    isAuthenticated: accounts.length > 0,
    user: account
      ? {
          name: account.name ?? '',
          email: account.username ?? '',
          givenName: (account.idTokenClaims?.given_name as string) ?? '',
          familyName: (account.idTokenClaims?.family_name as string) ?? '',
        }
      : null,
    login,
    logout,
    getAccessToken,
  };
}
