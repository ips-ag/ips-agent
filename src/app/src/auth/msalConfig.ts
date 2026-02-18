import { Configuration, LogLevel } from '@azure/msal-browser';

export const msalConfig: Configuration = {
  auth: {
    clientId: '17e121f3-96fa-43fc-948f-75d9798cead4',
    authority: 'https://login.microsoftonline.com/dcb58767-2d57-462f-82d5-552df1c47ccb',
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
  scopes: ['openid', 'profile', 'email', 'api://17e121f3-96fa-43fc-948f-75d9798cead4/FakeIntra'],
};

export const apiScopes = {
  scopes: ['api://17e121f3-96fa-43fc-948f-75d9798cead4/FakeIntra'],
};
