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

function getAuthHeaders(token?: string) {
  const headers: Record<string, string> = {}
  const activeToken = token || localStorage.getItem('fe_learn_token') || localStorage.getItem('access_token')
  if (activeToken) {
    headers['Authorization'] = `Bearer ${activeToken}`
  }
  return headers
}

export const QuizAPI = {
  loginWithGoogle(idToken: string) {
    return apiClient.post<{
      token: string
      user: {
        email: string
        name: string
        picture?: string
        isAdmin: boolean
        role: string
        unlockedSubjects?: string[]
      }
    }>('/api/auth/google', { idToken })
  },

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
    return apiClient.get<QuizQuestion[]>(`/api/quiz-sets/${id}/questions`, {
      params,
      headers
    })
  },

  submitQuiz(id: string, answers: Record<string, string[]>) {
    return apiClient.post<QuizSubmitResult>(`/api/quiz-sets/${id}/submit`, { answers })
  },

  unlockSubject(code: string) {
    const headers = getAuthHeaders()
    return apiClient.post<{ success: boolean; message: string; unlockedSubjects: string[] }>('/api/unlock', { code }, { headers })
  },

  getAdminKeys() {
    const headers = getAuthHeaders()
    return apiClient.get<{ keys: UnlockKeyItem[] }>('/api/admin/keys', { headers })
  },

  generateAdminKeys(payload: GenerateKeysPayload) {
    const headers = getAuthHeaders()
    return apiClient.post<{ success: boolean; message: string; keys: UnlockKeyItem[] }>('/api/admin/keys', {
      customCode: payload.customCode || payload.code,
      count: payload.batchCount || payload.count || 1,
      targetSubjects: payload.targetSubjects || ['jfe301', 'jit401']
    }, { headers })
  },

  deleteAdminKey(codeOrId: string) {
    const headers = getAuthHeaders()
    return apiClient.delete<{ success: boolean; message: string }>(`/api/admin/keys/${codeOrId}`, { headers })
  },

  getAdminUsers() {
    const headers = getAuthHeaders()
    return apiClient.get<{ users: AdminUserItem[] }>('/api/admin/users', { headers })
  },

  grantUserAccess(payload: GrantUserPayload) {
    const headers = getAuthHeaders()
    return apiClient.post<{ success: boolean; message: string; user: AdminUserItem }>('/api/admin/users/grant', payload, { headers })
  },

  getAdminQuizSets() {
    const headers = getAuthHeaders()
    return apiClient.get<{ sets: QuizSet[] }>('/api/admin/quiz-sets', { headers })
  },

  createQuizSet(data: {
    id: string
    code: string
    title: string
    description?: string
    color?: string
    isRestricted: boolean
  }) {
    const headers = getAuthHeaders()
    return apiClient.post<{ success: boolean; message: string; set: QuizSet }>('/api/admin/quiz-sets', data, { headers })
  },

  updateQuizSet(id: string, data: {
    code?: string
    title?: string
    description?: string
    color?: string
    isRestricted?: boolean
  }) {
    const headers = getAuthHeaders()
    return apiClient.put<{ success: boolean; message: string; set: QuizSet }>(`/api/admin/quiz-sets/${id}`, data, { headers })
  },

  toggleQuizSetRestriction(id: string, isRestricted: boolean) {
    const headers = getAuthHeaders()
    return apiClient.patch<{ success: boolean; message: string; set: QuizSet }>(`/api/admin/quiz-sets/${id}/restriction`, { isRestricted }, { headers })
  },

  deleteQuizSet(id: string) {
    const headers = getAuthHeaders()
    return apiClient.delete<{ success: boolean; message: string }>(`/api/admin/quiz-sets/${id}`, { headers })
  }
}
