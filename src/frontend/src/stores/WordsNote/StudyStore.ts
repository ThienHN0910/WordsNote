import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { StudyLocalService } from '@/services/WordsNote/StudyLocalService'
import type { CardDifficulty, StudyCard, StudyDeck } from '@/types/WordsNote'

export const useStudyStore = defineStore('study', () => {
  const decks = ref<StudyDeck[]>([])
  const cards = ref<StudyCard[]>([])

  function persist() {
    StudyLocalService.save({
      decks: decks.value,
      cards: cards.value,
    })
  }

  function load() {
    const snapshot = StudyLocalService.load()
    decks.value = snapshot.decks
    cards.value = snapshot.cards
  }

  const deckList = computed(() => [...decks.value].sort((a, b) => b.updatedAt.localeCompare(a.updatedAt)))

  function getDeckById(deckId: string) {
    return decks.value.find((deck) => deck.id === deckId)
  }

  function getDeckCards(deckId: string) {
    return cards.value.filter((card) => card.deckId === deckId)
  }

  function getDueCards(deckId: string) {
    const now = new Date()
    return getDeckCards(deckId).filter((card) => new Date(card.dueAt) <= now)
  }

  function getDeckStats(deckId: string) {
    return StudyLocalService.getDeckStats(deckId, cards.value)
  }

  function createDeck(title: string, description: string) {
    const deck = StudyLocalService.createDeck({ title, description })
    decks.value.push(deck)
    persist()
    return deck
  }

  function updateDeck(deckId: string, title: string, description: string) {
    const deck = getDeckById(deckId)
    if (!deck) return false

    deck.title = title.trim()
    deck.description = description.trim()
    deck.updatedAt = new Date().toISOString()
    persist()
    return true
  }

  function removeDeck(deckId: string) {
    decks.value = decks.value.filter((deck) => deck.id !== deckId)
    cards.value = cards.value.filter((card) => card.deckId !== deckId)
    persist()
  }

  function createCard(deckId: string, front: string, back: string, hint: string, tags: string[]) {
    const card = StudyLocalService.createCard({
      deckId,
      front,
      back,
      hint,
      tags,
    })
    cards.value.push(card)

    const deck = getDeckById(deckId)
    if (deck) {
      deck.updatedAt = new Date().toISOString()
    }

    persist()
    return card
  }

  function updateCard(cardId: string, front: string, back: string, hint: string, tags: string[]) {
    const card = cards.value.find((item) => item.id === cardId)
    if (!card) return false

    card.front = front.trim()
    card.back = back.trim()
    card.hint = hint.trim() || undefined
    card.tags = tags
    persist()
    return true
  }

  function removeCard(cardId: string) {
    cards.value = cards.value.filter((item) => item.id !== cardId)
    persist()
  }

  function reviewCard(cardId: string, difficulty: CardDifficulty) {
    const index = cards.value.findIndex((item) => item.id === cardId)
    if (index === -1) return false

    const updated = StudyLocalService.reviewCard(cards.value[index], difficulty)
    cards.value[index] = updated
    persist()
    return true
  }

  return {
    decks,
    cards,
    deckList,
    load,
    getDeckById,
    getDeckCards,
    getDueCards,
    getDeckStats,
    createDeck,
    updateDeck,
    removeDeck,
    createCard,
    updateCard,
    removeCard,
    reviewCard,
  }
})
