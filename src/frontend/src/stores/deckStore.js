import { defineStore } from 'pinia'
import deckService from '../services/deckService'

function normalizeDeck(deck) {
  if (!deck || typeof deck !== 'object') return deck

  return {
    ...deck,
    id: deck.id ?? deck.Id ?? deck.deck_id ?? null,
    name: deck.name ?? deck.Name ?? '',
    description: deck.description ?? deck.Description ?? '',
    cardCount: deck.cardCount ?? deck.CardCount ?? deck.card_count ?? 0,
    dueCardCount: deck.dueCardCount ?? deck.DueCardCount ?? deck.due_card_count ?? 0,
  }
}

export const useDeckStore = defineStore('deck', {
  state: () => ({
    decks: [],
    currentDeck: null,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchDecks() {
      this.loading = true
      this.error = null
      try {
        const response = await deckService.getDecks()
        this.decks = Array.isArray(response.data)
          ? response.data.map(normalizeDeck)
          : []
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to fetch decks'
      } finally {
        this.loading = false
      }
    },
    async fetchDeck(id) {
      this.loading = true
      this.error = null
      try {
        const response = await deckService.getDeck(id)
        this.currentDeck = normalizeDeck(response.data)
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to fetch deck'
      } finally {
        this.loading = false
      }
    },
    async createDeck(data) {
      this.loading = true
      this.error = null
      try {
        const response = await deckService.createDeck(data)
        const deck = normalizeDeck(response.data)
        this.decks.push(deck)
        return deck
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to create deck'
        throw err
      } finally {
        this.loading = false
      }
    },
    async updateDeck(id, data) {
      this.loading = true
      this.error = null
      try {
        const response = await deckService.updateDeck(id, data)
        const updatedDeck = normalizeDeck(response.data)
        const index = this.decks.findIndex((d) => d.id === id)
        if (index !== -1) this.decks[index] = updatedDeck
        if (this.currentDeck?.id === id) this.currentDeck = updatedDeck
        return updatedDeck
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to update deck'
        throw err
      } finally {
        this.loading = false
      }
    },
    async deleteDeck(id) {
      this.loading = true
      this.error = null
      try {
        await deckService.deleteDeck(id)
        this.decks = this.decks.filter((d) => d.id !== id)
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to delete deck'
        throw err
      } finally {
        this.loading = false
      }
    },
    async importCsv(id, file) {
      this.loading = true
      this.error = null
      try {
        await deckService.importCsv(id, file)
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to import CSV'
        throw err
      } finally {
        this.loading = false
      }
    },
    async resetDeck(id) {
      this.loading = true
      this.error = null
      try {
        await deckService.resetDeck(id)
      } catch (err) {
        this.error = err.response?.data?.message || 'Failed to reset deck'
        throw err
      } finally {
        this.loading = false
      }
    },
  },
})
