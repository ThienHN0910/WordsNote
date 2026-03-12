import api from './api'

export default {
  login: (username, password) => api.post('/api/auth/login', { username, password }),
  logout: () => {
    localStorage.removeItem('token')
  },
  getToken: () => localStorage.getItem('token'),
  isAuthenticated: () => !!localStorage.getItem('token'),
}
