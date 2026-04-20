import { UserAPI } from '@/apis/AS/UserAPI'

export interface AuthenticatedUserProfile {
  id?: string
  userName?: string
  email?: string
  name?: string
  role?: string
}

export const UserService = {
  async getMyProfile() {
    return UserAPI.getMe()
  },
}
