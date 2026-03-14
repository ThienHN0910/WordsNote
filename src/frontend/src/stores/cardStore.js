import { defineStore } from 'pinia'
import cardService from '../services/cardService'

function normalizeCard(card) {
  if (!card || typeof card !== 'object') return card

  return {
    ...card,
    id: card.id ?? card.Id ?? null,
    deckId: card.deckId ?? card.DeckId ?? card.deck_id ?? null,
    front: card.front ?? card.Front ?? '',
    back: card.back ?? card.Back ?? '',
    notes: card.notes ?? card.Notes ?? '',
    status: card.status ?? card.Status ?? 0,
    interval: card.interval ?? card.Interval ?? 0,
    nextReviewDate: card.nextReviewDate ?? card.NextReviewDate ?? null,
  }
}

export const useCardStore = defineStore('card', {
  state: () => ({
    cards: [],
    dueCards: [],
    loading: false,
    error: null,
  }),
  actions: {
    async fetchCards(deckId) {
      this.loading = true
      this.error = null
      try {
        const response = await cardService.getCards(deckId)
        this.cards = Array.isArray(response.data)
          ? response.data.map(normalizeCard)
          : []
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to fetch cards'
      } finally {
        this.loading = false
      }
    },
    async fetchDueCards() {
      this.loading = true
      this.error = null
      try {
        const response = await cardService.getDueCards()
        this.dueCards = Array.isArray(response.data)
          ? response.data.map(normalizeCard)
          : []
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to fetch due cards'
      } finally {
        this.loading = false
      }
    },
    async createCard(data) {
      this.loading = true
      this.error = null
      try {
        const response = await cardService.createCard(data)
        const card = normalizeCard(response.data)
        this.cards.push(card)
        return card
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to create card'
        throw err
      } finally {
        this.loading = false
      }
    },
    async reviewCard(id, result) {
      try {
        await cardService.reviewCard(id, result)
        this.dueCards = this.dueCards.filter((c) => c.id !== id)
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to review card'
        throw err
      }
    },
    async deleteCard(id) {
      this.loading = true
      this.error = null
      try {
        await cardService.deleteCard(id)
        this.cards = this.cards.filter((c) => c.id !== id)
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to delete card'
        throw err
      } finally {
        this.loading = false
      }
    },
  },
})
