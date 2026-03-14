import axios from 'axios';
import router from '@/router';
import { useAuthStore } from '@/stores/AS/AuthStore';

const authStore = useAuthStore();

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_API_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

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
      authStore.clearAuthToken();
      router.push({ name: 'login' }); 
    }
    return Promise.reject(error);
  }
);

export default apiClient;
