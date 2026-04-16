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
  async register(email: string, userName: string, password: string) {
    if (!email || !password) throw new Error('Email and password are required')
    return await AuthAPI.Register(email, userName, password)
  },
}
