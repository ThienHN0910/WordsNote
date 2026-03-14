import apiClient from '@/apis/apiClient';

export const InforAPI = {
  async getInfor() {
    return await apiClient.get('/api/Infor/get-info')
  },
}
