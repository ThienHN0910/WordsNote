import type { CreateUpdateUploadDto } from '@/dtos/DocType'
import axios from 'axios'
const base_url = import.meta.env.VITE_APP_API_URL
export const FileAPI = {
  async upload(params: CreateUpdateUploadDto) {
    return await axios.post(`${base_url}/api/File/upload`, params, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
  },
}
