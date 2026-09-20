import apiClient from '@/apis/apiClient'
import type { QuizQuestion, QuizSet, QuizSubmitResult, UnlockKeyItem, AdminUserItem } from '@/types/WordsNote'

export interface QuizQuestionsQueryParams {
  page?: number
  pageSize?: number
  exam?: boolean
  examCount?: number
  search?: string
}

export interface GenerateKeysPayload {
  customCode?: string
  code?: string
  batchCount?: number
  count?: number
  targetSubjects?: string[]
}

export interface GrantUserPayload {
  email: string
  subjects: string[]
}

function getAuthHeaders(adminSecret?: string, token?: string) {
  const headers: Record<string, string> = {}
  const activeToken = token || localStorage.getItem('fe_learn_token') || localStorage.getItem('access_token')
  if (activeToken) {
    headers['Authorization'] = `Bearer ${activeToken}`
  }
  if (adminSecret && adminSecret.trim()) {
    headers['x-admin-secret'] = adminSecret.trim()
  }
  const userStr = localStorage.getItem('fe_learn_user')
  if (userStr) {
    try {
      const u = JSON.parse(userStr)
      if (u?.email) headers['x-user-email'] = u.email
    } catch {}
  }
  return headers
}

export const QuizAPI = {
  getQuizSets() {
    const headers = getAuthHeaders()
    return apiClient.get<QuizSet[]>('/api/quiz-sets', { headers })
  },

  getCatalog() {
    const headers = getAuthHeaders()
    return apiClient.get<{ subjects: QuizSet[] }>('/api/catalog', { headers })
  },

  getQuizSet(id: string) {
    const headers = getAuthHeaders()
    return apiClient.get<QuizSet>(`/api/quiz-sets/${id}`, { headers })
  },

  getQuestions(id: string, params?: QuizQuestionsQueryParams) {
    const headers = getAuthHeaders()
    return apiClient.get<QuizQuestion[]>(`/api/quiz-sets/${id}/questions`, { params, headers })
  },

  submitQuiz(id: string, answers: Record<string, string[]>) {
    return apiClient.post<QuizSubmitResult>(`/api/quiz-sets/${id}/submit`, { answers })
  },

  unlockSubject(code: string) {
    const headers = getAuthHeaders()
    return apiClient.post<{ success: boolean; message: string; unlockedSubjects: string[] }>('/api/unlock', { code }, { headers })
  },

  getAdminKeys(adminSecret?: string) {
    const headers = getAuthHeaders(adminSecret)
    return apiClient.get<{ keys: UnlockKeyItem[] }>('/api/admin/keys', { headers })
  },

  generateAdminKeys(payload: GenerateKeysPayload, adminSecret?: string) {
    const headers = getAuthHeaders(adminSecret)
    return apiClient.post<{ success: boolean; message: string; keys: UnlockKeyItem[] }>('/api/admin/keys', {
      customCode: payload.customCode || payload.code,
      count: payload.batchCount || payload.count || 1,
      targetSubjects: payload.targetSubjects || ['jfe301', 'jit401']
    }, { headers })
  },

  deleteAdminKey(codeOrId: string, adminSecret?: string) {
    const headers = getAuthHeaders(adminSecret)
    return apiClient.delete<{ success: boolean; message: string }>(`/api/admin/keys/${codeOrId}`, { headers })
  },

  getAdminUsers(adminSecret?: string) {
    const headers = getAuthHeaders(adminSecret)
    return apiClient.get<{ users: AdminUserItem[] }>('/api/admin/users', { headers })
  },

  grantUserAccess(payload: GrantUserPayload, adminSecret?: string) {
    const headers = getAuthHeaders(adminSecret)
    return apiClient.post<{ success: boolean; message: string; user: AdminUserItem }>('/api/admin/users/grant', payload, { headers })
  }
}
