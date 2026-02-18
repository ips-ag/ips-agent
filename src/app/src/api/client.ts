import axios from 'axios';
import { InteractionRequiredAuthError } from '@azure/msal-browser';
import { apiScopes } from '../auth/msalConfig';
import { msalInstance } from '../auth/msalInstance';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api/v1` : '/api/v1',
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor — acquire token silently and attach as Bearer header
apiClient.interceptors.request.use(async (config) => {
  const accounts = msalInstance.getAllAccounts();
  if (accounts.length > 0) {
    try {
      const response = await msalInstance.acquireTokenSilent({
        ...apiScopes,
        account: accounts[0],
      });
      config.headers.Authorization = `Bearer ${response.accessToken}`;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        await msalInstance.acquireTokenRedirect(apiScopes);
      }
    }
  }
  return config;
});

// Response interceptor — handle errors
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      await msalInstance.loginRedirect();
    }
    return Promise.reject(error);
  },
);

export default apiClient;
