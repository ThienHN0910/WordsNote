import apiClient from '@/apis/apiClient'
import type { DownloadPageOverride } from '@/types/FFP/AppDownload'

export const DownloadConfigAPI = {
  getConfig() {
    return apiClient.get<DownloadPageOverride>('/api/download-config')
  },

  upsertConfig(payload: DownloadPageOverride) {
    return apiClient.put<DownloadPageOverride>('/api/download-config', payload)
  },

  resetConfig() {
    return apiClient.delete('/api/download-config')
  },
}