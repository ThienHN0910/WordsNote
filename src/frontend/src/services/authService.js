import { supabase } from './supabaseClient'
import api from './api'

const TOKEN_KEY = 'token'

export default {
  async loginWithGoogle() {
    const redirectTo = import.meta.env.VITE_SUPABASE_REDIRECT_URL || `${window.location.origin}/login`
    const { error } = await supabase.auth.signInWithOAuth({
      provider: 'google',
      options: { redirectTo },
    })
    if (error) {
      throw error
    }
  },

  async syncSession() {
    const { data, error } = await supabase.auth.getSession()
    if (error) {
      localStorage.removeItem(TOKEN_KEY)
      return null
    }

    const accessToken = data.session?.access_token || null
    if (accessToken) {
      localStorage.setItem(TOKEN_KEY, accessToken)
    } else {
      localStorage.removeItem(TOKEN_KEY)
    }

    return accessToken
  },

  async validateBackendToken() {
    const token = localStorage.getItem(TOKEN_KEY)
    if (!token) return false

    try {
      await api.get('/api/auth/me', { skipAuthRedirect: true })
      return true
    } catch {
      localStorage.removeItem(TOKEN_KEY)
      return false
    }
  },

  async logout() {
    await supabase.auth.signOut()
    localStorage.removeItem(TOKEN_KEY)
  },

  getToken: () => localStorage.getItem(TOKEN_KEY),
  isAuthenticated: () => !!localStorage.getItem(TOKEN_KEY),
}
