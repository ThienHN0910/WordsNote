import axios from 'axios';
import router from '@/router';
import { useAuthStore } from '@/stores/AS/AuthStore';
import { pinia } from '@/stores/pinia';

const authStore = useAuthStore(pinia);

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_API_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

function getRequestPath(rawUrl: string | undefined) {
  if (!rawUrl) {
    return '';
  }

  const normalized = rawUrl.toLowerCase();
  const queryIndex = normalized.indexOf('?');
  return queryIndex >= 0 ? normalized.slice(0, queryIndex) : normalized;
}

function isProtectedApiRequest(config: { url?: string; method?: string } | undefined) {
  const path = getRequestPath(config?.url);
  const method = String(config?.method || 'get').toLowerCase();

  if (!path) {
    return authStore.hasAuthSession;
  }

  if (path.startsWith('/api/auth')) {
    return false;
  }

  if (path.startsWith('/api/study') || path.startsWith('/api/tests') || path.startsWith('/api/user')) {
    return true;
  }

  if (path.startsWith('/api/collections') || path.startsWith('/api/cards')) {
    return method !== 'get';
  }

  return authStore.hasAuthSession;
}

apiClient.interceptors.request.use(config => {
  const token = authStore.auth_token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, error => {
  return Promise.reject(error);
});

apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error.response && error.response.status === 401) {
      const shouldHandleAuthFailure =
        isProtectedApiRequest(error.config) ||
        router.currentRoute.value.matched.some(record => record.meta.requiresAuth);

      if (shouldHandleAuthFailure) {
        authStore.clearAuthToken();
      }

      if (shouldHandleAuthFailure && router.currentRoute.value.name !== 'login') {
        router.push({
          name: 'login',
          query: {
            redirect: router.currentRoute.value.fullPath,
          },
        });
      }
    }
    return Promise.reject(error);
  }
);

export default apiClient;
