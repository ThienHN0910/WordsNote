import axios from 'axios'

const base_url = import.meta.env.VITE_APP_API_URL
export const DocAPI = {
  async getDocList() {
    return await axios.get(`${base_url}/api/Doc/getAllDocs`)
  },
}
