import { InforAPI } from '@/apis/FFP/InforAPI'
export const InforService = {
  async getInfor() {
    return await InforAPI.getInfor()
  },
}
