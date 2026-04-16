import apiClient from '@/apis/apiClient';

export const AuthAPI = {
  async LoginByEmail(email: string, password: string) {
    return await apiClient.post('/api/Auth/login', { email, password });
  },
  async LoginByUsername(username: string, password: string) {
    return await apiClient.post('/api/Auth/login', { username, password });
  },
  async LoginWithGoogle(idToken: string) {
    return await apiClient.post('/api/Auth/google', { idToken });
  }
};
