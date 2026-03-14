import axios from 'axios'
import type { DocumentDto } from '@/dtos/DocType'

const base_url = import.meta.env.VITE_APP_API_URL
export const DocService = {
  async getDocList(): Promise<DocumentDto[]> {
    const res = await axios.get(`${base_url}/api/Doc/getAllDocs`)
    return res.data
  },
}
