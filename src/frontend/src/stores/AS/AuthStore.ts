import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    auth_token: '',
    isAuthenticated: false,
  }),
  actions: {
    setAuthToken(token: string) {
      this.auth_token = token
      this.isAuthenticated = true
    },
    clearAuthToken() {
      this.auth_token = ''
      this.isAuthenticated = false
    },
  },
  persist: {
    storage: sessionStorage,
    paths: ['auth_token', 'isAuthenticated'],
  },
})