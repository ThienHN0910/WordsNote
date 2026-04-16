import apiClient from '@/apis/apiClient'
import type { DeepStudyAnswerResult, StudyCard, StudyDeck } from '@/types/WordsNote'

interface CardUpsertPayload {
  collectionId: string
  front: string
  back: string
  hint: string
  tags: string[]
}

interface ImportCardsResult {
  imported: number
  skipped: number
}

export const StudyAPI = {
  getDecks() {
    return apiClient.get<StudyDeck[]>('/api/collections')
  },

  createDeck(title: string, description: string) {
    return apiClient.post<StudyDeck>('/api/collections', { title, description })
  },

  updateDeck(deckId: string, title: string, description: string) {
    return apiClient.put<StudyDeck>(`/api/collections/${deckId}`, { title, description })
  },

  deleteDeck(deckId: string) {
    return apiClient.delete(`/api/collections/${deckId}`)
  },

  getCards(collectionId?: string) {
    return apiClient.get<StudyCard[]>('/api/cards', {
      params: collectionId ? { collectionId } : undefined,
    })
  },

  createCard(payload: CardUpsertPayload) {
    return apiClient.post<StudyCard>('/api/cards', payload)
  },

  updateCard(cardId: string, payload: CardUpsertPayload) {
    return apiClient.put<StudyCard>(`/api/cards/${cardId}`, payload)
  },

  deleteCard(cardId: string) {
    return apiClient.delete(`/api/cards/${cardId}`)
  },

  importCards(collectionId: string, rawText: string) {
    return apiClient.post<ImportCardsResult>('/api/cards/import', { collectionId, rawText })
  },

  reviewCard(cardId: string, difficulty: 'easy' | 'medium' | 'hard') {
    return apiClient.post<StudyCard>('/api/study/review', { cardId, difficulty })
  },

  checkDeepAnswer(cardId: string, answer: string) {
    return apiClient.post<DeepStudyAnswerResult>('/api/study/deep/answer', { cardId, answer })
  },
}