import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    auth_token: '',
    isAuthenticated: false,
  }),
  getters: {
    hasAuthSession: (state) => Boolean(state.auth_token),
  },
  actions: {
    setAuthToken(token: string) {
      const normalizedToken = token.trim()
      this.auth_token = normalizedToken
      this.isAuthenticated = Boolean(normalizedToken)
    },
    clearAuthToken() {
      this.auth_token = ''
      this.isAuthenticated = false
    },
    rehydrateFromPersistedState() {
      if (!this.auth_token) {
        const legacyPayload = sessionStorage.getItem(this.$id)
        if (legacyPayload) {
          try {
            const parsed = JSON.parse(legacyPayload) as Partial<{ auth_token: string }>
            const legacyToken = String(parsed.auth_token || '').trim()
            if (legacyToken) {
              this.auth_token = legacyToken
            }
          } catch {
            // Ignore malformed legacy session payloads.
          } finally {
            sessionStorage.removeItem(this.$id)
          }
        }
      }

      this.isAuthenticated = Boolean(this.auth_token)
    },
  },
  persist: {
    storage: localStorage,
    pick: ['auth_token'],
  },
})
