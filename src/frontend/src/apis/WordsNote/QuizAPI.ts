import apiClient from '@/apis/apiClient'
import type { QuizQuestion, QuizSet, QuizSubmitResult } from '@/types/WordsNote'

export interface QuizQuestionsQueryParams {
  page?: number
  pageSize?: number
  exam?: boolean
  examCount?: number
  search?: string
}

export const QuizAPI = {
  getQuizSets() {
    return apiClient.get<QuizSet[]>('/api/quiz-sets')
  },

  getQuizSet(id: string) {
    return apiClient.get<QuizSet>(`/api/quiz-sets/${id}`)
  },

  getQuestions(id: string, params?: QuizQuestionsQueryParams) {
    return apiClient.get<QuizQuestion[]>(`/api/quiz-sets/${id}/questions`, { params })
  },

  submitQuiz(id: string, answers: Record<string, string[]>) {
    return apiClient.post<QuizSubmitResult>(`/api/quiz-sets/${id}/submit`, { answers })
  },
}
