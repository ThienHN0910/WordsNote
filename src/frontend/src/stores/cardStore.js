import { defineStore } from 'pinia'
import cardService from '../services/cardService'

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
        this.cards = response.data
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to fetch cards'
      } finally {
        this.loading = false
      }
    },
    async fetchDueCards(deckId) {
      this.loading = true
      this.error = null
      try {
        const response = await cardService.getDueCards(deckId)
        this.dueCards = response.data
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
        this.cards.push(response.data)
        return response.data
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
