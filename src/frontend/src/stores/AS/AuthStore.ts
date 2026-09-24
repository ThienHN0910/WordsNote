import { defineStore } from 'pinia'

const AUTH_TOKEN_STORAGE_KEY = 'wordsnote_auth_token'

export interface AuthUser {
  email: string
  name: string
  picture?: string
  isAdmin?: boolean
  role?: string
}

const AUTH_USER_STORAGE_KEY = 'wordsnote_user'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    auth_token: '',
    isAuthenticated: false,
    user: null as AuthUser | null,
  }),
  getters: {
    hasAuthSession: (state) => Boolean(state.auth_token),
    currentUser: (state) => state.user,
  },
  actions: {
    setAuthSession(tokenPayload: unknown, userPayload?: AuthUser | null) {
      const normalizedToken = extractToken(tokenPayload)
      this.auth_token = normalizedToken
      this.isAuthenticated = Boolean(normalizedToken)

      if (userPayload) {
        this.user = userPayload
        persistUser(userPayload)
      } else if (!this.user && tokenPayload && typeof tokenPayload === 'object') {
        const candidateUser = (tokenPayload as any).user || (tokenPayload as any).User
        if (candidateUser && candidateUser.email) {
          const mappedUser: AuthUser = {
            email: candidateUser.email,
            name: candidateUser.name || candidateUser.email.split('@')[0],
            picture: candidateUser.picture,
            isAdmin: Boolean(candidateUser.isAdmin),
            role: candidateUser.role || (candidateUser.isAdmin ? 'Admin' : 'User')
          }
          this.user = mappedUser
          persistUser(mappedUser)
        }
      }

      if (normalizedToken) {
        persistToken(normalizedToken)
        try {
          localStorage.setItem('fe_learn_token', normalizedToken)
          if (this.user) {
            localStorage.setItem('fe_learn_user', JSON.stringify(this.user))
          }
        } catch {
          // ignore
        }
      } else {
        clearPersistedToken()
      }
    },
    setAuthToken(token: unknown) {
      this.setAuthSession(token)
    },
    clearAuthToken() {
      this.auth_token = ''
      this.isAuthenticated = false
      this.user = null
      clearPersistedToken()
      try {
        localStorage.removeItem('fe_learn_token')
        localStorage.removeItem('fe_learn_user')
        localStorage.removeItem('access_token')
      } catch {
        // ignore
      }
    },
    rehydrateFromPersistedState() {
      this.auth_token = normalizeToken(this.auth_token)

      if (!this.auth_token) {
        this.auth_token = readPersistedToken()
      }

      if (!this.auth_token) {
        try {
          const fallbackToken = localStorage.getItem('fe_learn_token') || localStorage.getItem('access_token')
          if (fallbackToken) {
            this.auth_token = normalizeToken(fallbackToken)
          }
        } catch {
          // ignore
        }
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

      if (!this.user) {
        this.user = readPersistedUser()
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

function readPersistedUser(): AuthUser | null {
  if (typeof window === 'undefined') return null
  try {
    const raw = localStorage.getItem(AUTH_USER_STORAGE_KEY) || localStorage.getItem('fe_learn_user')
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

function persistUser(user: AuthUser) {
  if (typeof window === 'undefined') return
  try {
    localStorage.setItem(AUTH_USER_STORAGE_KEY, JSON.stringify(user))
  } catch {
    // ignore
  }
}

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
