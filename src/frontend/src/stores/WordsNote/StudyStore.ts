import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { StudyAPI } from '@/apis/WordsNote/StudyAPI'
import type { CardDifficulty, DeepStudyAnswerResult, StudyCard, StudyDeck } from '@/types/WordsNote'

interface ImportCardsResult {
  imported: number
  skipped: number
}

export const useStudyStore = defineStore('study', () => {
  const decks = ref<StudyDeck[]>([])
  const cards = ref<StudyCard[]>([])

  function resolveCollectionId(card: StudyCard) {
    return card.collectionId || card.deckId || ''
  }

  async function load() {
    const [decksResponse, cardsResponse] = await Promise.all([StudyAPI.getDecks(), StudyAPI.getCards()])
    decks.value = decksResponse.data
    cards.value = cardsResponse.data
  }

  const deckList = computed(() => [...decks.value].sort((a, b) => b.updatedAt.localeCompare(a.updatedAt)))

  function getDeckById(deckId: string) {
    return decks.value.find((deck) => deck.id === deckId)
  }

  function getDeckCards(deckId: string) {
    return cards.value.filter((card) => resolveCollectionId(card) === deckId)
  }

  function getDueCards(deckId: string) {
    const now = new Date()
    return getDeckCards(deckId).filter((card) => new Date(card.dueAt) <= now)
  }

  function getDeckStats(deckId: string) {
    const now = new Date()
    const deckCards = cards.value.filter((card) => resolveCollectionId(card) === deckId)
    const dueCards = deckCards.filter((card) => new Date(card.dueAt) <= now).length
    const masteredCards = deckCards.filter((card) => card.streak >= 5).length

    return {
      totalCards: deckCards.length,
      dueCards,
      masteredCards,
    }
  }

  async function createDeck(title: string, description: string) {
    const response = await StudyAPI.createDeck(title, description)
    const deck = response.data
    decks.value = [deck, ...decks.value]
    return deck
  }

  async function updateDeck(deckId: string, title: string, description: string) {
    const response = await StudyAPI.updateDeck(deckId, title, description)
    const updatedDeck = response.data
    const index = decks.value.findIndex((deck) => deck.id === deckId)
    if (index === -1) return false

    decks.value[index] = updatedDeck
    return true
  }

  async function removeDeck(deckId: string) {
    await StudyAPI.deleteDeck(deckId)
    decks.value = decks.value.filter((deck) => deck.id !== deckId)
    cards.value = cards.value.filter((card) => resolveCollectionId(card) !== deckId)
  }

  async function createCard(deckId: string, front: string, back: string, hint: string, tags: string[]) {
    const response = await StudyAPI.createCard({
      collectionId: deckId,
      front,
      back,
      hint,
      tags,
    })
    const card = response.data
    cards.value.push(card)
    await syncDecks()
    return card
  }

  async function updateCard(cardId: string, front: string, back: string, hint: string, tags: string[]) {
    const card = cards.value.find((item) => item.id === cardId)
    if (!card) return false

    const response = await StudyAPI.updateCard(cardId, {
      collectionId: resolveCollectionId(card),
      front,
      back,
      hint,
      tags,
    })

    const index = cards.value.findIndex((item) => item.id === cardId)
    if (index === -1) return false

    cards.value[index] = response.data
    await syncDecks()
    return true
  }

  async function removeCard(cardId: string) {
    await StudyAPI.deleteCard(cardId)
    cards.value = cards.value.filter((item) => item.id !== cardId)
    await syncDecks()
  }

  async function reviewCard(cardId: string, difficulty: CardDifficulty) {
    const index = cards.value.findIndex((item) => item.id === cardId)
    if (index === -1) return false

    const response = await StudyAPI.reviewCard(cardId, difficulty)
    cards.value[index] = response.data
    await syncDecks()
    return true
  }

  async function importCardsFromText(deckId: string, rawText: string): Promise<ImportCardsResult> {
    const response = await StudyAPI.importCards(deckId, rawText)
    const cardsResponse = await StudyAPI.getCards(deckId)
    const otherCards = cards.value.filter((card) => resolveCollectionId(card) !== deckId)
    cards.value = [...otherCards, ...cardsResponse.data]
    await syncDecks()
    return response.data
  }

  async function checkDeepAnswer(cardId: string, answer: string): Promise<DeepStudyAnswerResult> {
    const response = await StudyAPI.checkDeepAnswer(cardId, answer)
    return response.data
  }

  async function syncDecks() {
    const response = await StudyAPI.getDecks()
    decks.value = response.data
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
    checkDeepAnswer,
    importCardsFromText,
  }
})
