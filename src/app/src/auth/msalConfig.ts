import { Configuration, LogLevel } from '@azure/msal-browser';

const clientId = import.meta.env.VITE_AUTH_CLIENT_ID as string;
const tenantId = import.meta.env.VITE_AUTH_TENANT_ID as string;

export const msalConfig: Configuration = {
  auth: {
    clientId: clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: '/',
    postLogoutRedirectUri: '/',
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      logLevel: LogLevel.Warning,
    },
  },
};

export const loginRequest = {
  scopes: ['openid', 'profile', 'email', `api://${clientId}/FakeIntra`],
};

export const apiScopes = {
  scopes: [`api://${clientId}/FakeIntra`],
};
