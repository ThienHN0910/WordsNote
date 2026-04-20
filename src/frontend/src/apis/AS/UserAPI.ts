import apiClient from '@/apis/apiClient'

export const UserAPI = {
  getMe() {
    return apiClient.get('/api/user/me')
  },
}
