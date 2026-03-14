import axios from 'axios'
import type { DocVersion } from '@/dtos/DocType'

const base_url = import.meta.env.VITE_APP_API_URL
export const VersionService = {
  async getVersionByDocumentId(documentId: string): Promise<DocVersion[]> {
    const res = await axios.get(`${base_url}/api/Version/get-all-version`, {
      params: { docId: documentId },
    })
    return res.data
  },
}
