import { defineStore } from 'pinia'
import authService from '../services/authService'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token'),
    user: null,
    loading: false,
    error: null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
  actions: {
    async login(username, password) {
      this.loading = true
      this.error = null
      try {
        const response = await authService.login(username, password)
        this.token = response.data.token
        localStorage.setItem('token', this.token)
      } catch (err) {
        this.error = err.response?.data?.message || 'Login failed'
        throw err
      } finally {
        this.loading = false
      }
    },
    logout() {
      this.token = null
      this.user = null
      authService.logout()
    },
  },
})
