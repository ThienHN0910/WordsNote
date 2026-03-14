import apiClient from '@/apis/apiClient'
import type { StudyCard, StudyDeck } from '@/types/WordsNote'

interface CardUpsertPayload {
  deskId: string
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
    return apiClient.get<StudyDeck[]>('/api/desk')
  },

  createDeck(title: string, description: string) {
    return apiClient.post<StudyDeck>('/api/desk', { title, description })
  },

  updateDeck(deckId: string, title: string, description: string) {
    return apiClient.put<StudyDeck>(`/api/desk/${deckId}`, { title, description })
  },

  deleteDeck(deckId: string) {
    return apiClient.delete(`/api/desk/${deckId}`)
  },

  getCards(deckId?: string) {
    return apiClient.get<StudyCard[]>('/api/card', {
      params: deckId ? { deckId } : undefined,
    })
  },

  createCard(payload: CardUpsertPayload) {
    return apiClient.post<StudyCard>('/api/card', payload)
  },

  updateCard(cardId: string, payload: CardUpsertPayload) {
    return apiClient.put<StudyCard>(`/api/card/${cardId}`, payload)
  },

  deleteCard(cardId: string) {
    return apiClient.delete(`/api/card/${cardId}`)
  },

  importCards(deckId: string, rawText: string) {
    return apiClient.post<ImportCardsResult>('/api/card/import', { deskId: deckId, rawText })
  },

  reviewCard(cardId: string, difficulty: 'easy' | 'medium' | 'hard') {
    return apiClient.post<StudyCard>(`/api/card/${cardId}/review`, { difficulty })
  },
}