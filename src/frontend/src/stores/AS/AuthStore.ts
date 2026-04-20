import { defineStore } from 'pinia'

const AUTH_TOKEN_STORAGE_KEY = 'wordsnote_auth_token'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    auth_token: '',
    isAuthenticated: false,
  }),
  getters: {
    hasAuthSession: (state) => Boolean(state.auth_token),
  },
  actions: {
    setAuthToken(token: unknown) {
      const normalizedToken = extractToken(token)
      this.auth_token = normalizedToken
      this.isAuthenticated = Boolean(normalizedToken)

      if (normalizedToken) {
        persistToken(normalizedToken)
      } else {
        clearPersistedToken()
      }
    },
    clearAuthToken() {
      this.auth_token = ''
      this.isAuthenticated = false
      clearPersistedToken()
    },
    rehydrateFromPersistedState() {
      this.auth_token = normalizeToken(this.auth_token)

      if (!this.auth_token) {
        this.auth_token = readPersistedToken()
      }

      if (!this.auth_token) {
        const legacyPayload = sessionStorage.getItem(this.$id)
        if (legacyPayload) {
          try {
            const parsed = JSON.parse(legacyPayload) as Partial<{ auth_token: string }>
            const legacyToken = normalizeToken(parsed.auth_token)
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

      if (this.auth_token) {
        persistToken(this.auth_token)
      }
    },
  },
  persist: {
    storage: localStorage,
    pick: ['auth_token'],
  },
})

function extractToken(raw: unknown) {
  if (typeof raw === 'string') {
    return normalizeToken(raw)
  }

  if (raw && typeof raw === 'object') {
    const candidate = raw as Partial<{ token: unknown; data: unknown; auth_token: unknown }>
    const fromToken = normalizeToken(candidate.token)
    if (fromToken) {
      return fromToken
    }

    const fromData = normalizeToken(candidate.data)
    if (fromData) {
      return fromData
    }

    const fromAuthToken = normalizeToken(candidate.auth_token)
    if (fromAuthToken) {
      return fromAuthToken
    }
  }

  return ''
}

function normalizeToken(raw: unknown) {
  const token = typeof raw === 'string' ? raw.trim() : ''
  if (!token) {
    return ''
  }

  // Accept standard JWT shape only to prevent persisting malformed values like [object Object].
  const jwtParts = token.split('.')
  return jwtParts.length === 3 && jwtParts.every((part) => part.length > 0) ? token : ''
}

function readPersistedToken() {
  if (typeof window === 'undefined') {
    return ''
  }

  return normalizeToken(window.localStorage.getItem(AUTH_TOKEN_STORAGE_KEY))
}

function persistToken(token: string) {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, token)
}

function clearPersistedToken() {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.removeItem(AUTH_TOKEN_STORAGE_KEY)
}
