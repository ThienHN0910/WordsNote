import { AuthAPI } from '@/apis/AS/AuthAPI'
import { supabaseClient } from '@/services/AS/SupabaseClient'
export const AuthService = {
  async login(email: string, password: string) {
    if (!email || !password) throw new Error('Email and password are required')
    if (email.includes('@')){
        return await AuthAPI.LoginByEmail(email, password)
    }else{
        return await AuthAPI.LoginByUsername(email, password)
    }
  },
  async register(email: string, userName: string, password: string) {
    if (!email || !password) throw new Error('Email and password are required')
    return await AuthAPI.Register(email, userName, password)
  },
  async loginWithGoogle() {
    const redirectTo = `${window.location.origin}/login`
    const { error } = await supabaseClient.auth.signInWithOAuth({
      provider: 'google',
      options: { redirectTo },
    })
    if (error) throw error
  },
  async getSupabaseAccessToken() {
    const { data, error } = await supabaseClient.auth.getSession()
    if (error) throw error
    return data.session?.access_token ?? ''
  },
  async logoutSupabase() {
    await supabaseClient.auth.signOut()
  },
}
