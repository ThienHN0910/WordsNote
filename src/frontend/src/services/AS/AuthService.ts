import { AuthAPI } from '@/apis/AS/AuthAPI'
export const AuthService = {
  async login(email: string, password: string) {
    if (!email || !password) throw new Error('Email and password are required')
    if (email.includes('@')){
        return await AuthAPI.LoginByEmail(email, password)
    }else{
        return await AuthAPI.LoginByUsername(email, password)
    }
  },
  async loginWithGoogle(idToken: string) {
    if (!idToken) throw new Error('Google ID token is required')
    return await AuthAPI.LoginWithGoogle(idToken)
  },
}
