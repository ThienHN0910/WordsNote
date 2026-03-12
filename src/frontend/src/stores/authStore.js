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
    async syncSession() {
      this.token = await authService.syncSession()
      this.error = null
    },

    async loginWithGoogle() {
      this.loading = true
      this.error = null
      try {
        await authService.loginWithGoogle()
      } catch (err) {
        this.error = err.message || 'Google login failed'
        throw err
      } finally {
        this.loading = false
      }
    },

    async logout() {
      this.token = null
      this.user = null
      await authService.logout()
    },
  },
})
