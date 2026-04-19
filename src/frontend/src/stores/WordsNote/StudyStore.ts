import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { StudyAPI } from '@/apis/WordsNote/StudyAPI'
import type { CardDifficulty, DeepStudyAnswerResult, StudyCard, StudyDeck } from '@/types/WordsNote'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { pinia } from '@/stores/pinia'
import {
  loadLocalStudySnapshot,
  makeLocalCard,
  makeLocalDeck,
  saveLocalStudySnapshot,
  touchLocalDeck,
} from '@/services/WordsNote/localManageStorage'

interface ImportCardsResult {
  imported: number
  skipped: number
}

type StudyLoadSource = 'cloud' | 'local'

type CloudDeckPayload = Partial<StudyDeck> & {
  Id?: string
}

type CloudCardPayload = Partial<StudyCard> & {
  Id?: string
  CollectionId?: string
  DeckId?: string
}

export const useStudyStore = defineStore('study', () => {
  const authStore = useAuthStore(pinia)
  const decks = ref<StudyDeck[]>([])
  const cards = ref<StudyCard[]>([])
  const lastLoadSource = ref<StudyLoadSource>('local')

  function normalizeRemoteDeck(rawDeck: CloudDeckPayload): StudyDeck {
    const now = new Date().toISOString()

    return {
      id: String(rawDeck.id ?? rawDeck.Id ?? '').trim(),
      title: String(rawDeck.title ?? '').trim(),
      description: String(rawDeck.description ?? '').trim(),
      createdAt: String(rawDeck.createdAt ?? now),
      updatedAt: String(rawDeck.updatedAt ?? rawDeck.createdAt ?? now),
    }
  }

  function normalizeRemoteCard(rawCard: CloudCardPayload): StudyCard {
    const now = new Date().toISOString()
    const collectionId = String(
      rawCard.collectionId ?? rawCard.CollectionId ?? rawCard.deckId ?? rawCard.DeckId ?? '',
    ).trim()

    return {
      id: String(rawCard.id ?? rawCard.Id ?? '').trim(),
      collectionId,
      deckId: collectionId || undefined,
      front: String(rawCard.front ?? '').trim(),
      back: String(rawCard.back ?? '').trim(),
      hint: rawCard.hint ? String(rawCard.hint).trim() : undefined,
      tags: Array.isArray(rawCard.tags) ? rawCard.tags.map((tag) => String(tag).trim()).filter(Boolean) : [],
      dueAt: String(rawCard.dueAt ?? now),
      lastReviewedAt: rawCard.lastReviewedAt ? String(rawCard.lastReviewedAt) : undefined,
      streak: Number(rawCard.streak ?? 0),
    }
  }

  function setCloudSnapshot(rawDecks: StudyDeck[] | CloudDeckPayload[], rawCards: StudyCard[] | CloudCardPayload[]) {
    decks.value = rawDecks
      .map((deck) => normalizeRemoteDeck(deck))
      .filter((deck) => Boolean(deck.id))

    cards.value = rawCards
      .map((card) => normalizeRemoteCard(card))
      .filter((card) => Boolean(card.id && card.collectionId))

    lastLoadSource.value = 'cloud'
  }

  function setLocalSnapshot() {
    const snapshot = loadLocalStudySnapshot()
    decks.value = snapshot.decks
    cards.value = snapshot.cards
    lastLoadSource.value = 'local'
  }

  function resolveCollectionId(card: StudyCard) {
    return card.collectionId || card.deckId || ''
  }

  function isCloudMode() {
    return authStore.isAuthenticated && Boolean(authStore.auth_token)
  }

  function saveLocalSnapshot() {
    saveLocalStudySnapshot({
      decks: decks.value,
      cards: cards.value,
    })
  }

  async function load() {
    if (isCloudMode()) {
      const [decksResponse, cardsResponse] = await Promise.all([StudyAPI.getDecks(), StudyAPI.getCards()])
      setCloudSnapshot(decksResponse.data, cardsResponse.data)
      return
    }

    setLocalSnapshot()
  }

  async function loadForLearn() {
    try {
      const [decksResponse, cardsResponse] = await Promise.all([StudyAPI.getDecks(), StudyAPI.getCards()])
      setCloudSnapshot(decksResponse.data, cardsResponse.data)
      return
    } catch {
      setLocalSnapshot()
    }
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
    if (!isCloudMode()) {
      const deck = makeLocalDeck(title, description)
      decks.value = [deck, ...decks.value]
      saveLocalSnapshot()
      return deck
    }

    const response = await StudyAPI.createDeck(title, description)
    const deck = response.data
    decks.value = [deck, ...decks.value]
    return deck
  }

  async function updateDeck(deckId: string, title: string, description: string) {
    if (!isCloudMode()) {
      const index = decks.value.findIndex((deck) => deck.id === deckId)
      if (index === -1) return false

      decks.value[index] = touchLocalDeck({
        ...decks.value[index],
        title: title.trim(),
        description: description.trim(),
      })
      saveLocalSnapshot()
      return true
    }

    const response = await StudyAPI.updateDeck(deckId, title, description)
    const updatedDeck = response.data
    const index = decks.value.findIndex((deck) => deck.id === deckId)
    if (index === -1) return false

    decks.value[index] = updatedDeck
    return true
  }

  async function removeDeck(deckId: string) {
    if (!isCloudMode()) {
      decks.value = decks.value.filter((deck) => deck.id !== deckId)
      cards.value = cards.value.filter((card) => resolveCollectionId(card) !== deckId)
      saveLocalSnapshot()
      return
    }

    await StudyAPI.deleteDeck(deckId)
    decks.value = decks.value.filter((deck) => deck.id !== deckId)
    cards.value = cards.value.filter((card) => resolveCollectionId(card) !== deckId)
  }

  async function createCard(deckId: string, front: string, back: string, hint: string, tags: string[]) {
    if (!isCloudMode()) {
      const card = makeLocalCard({
        collectionId: deckId,
        front,
        back,
        hint,
        tags,
      })
      cards.value.push(card)

      const deckIndex = decks.value.findIndex((deck) => deck.id === deckId)
      if (deckIndex !== -1) {
        decks.value[deckIndex] = touchLocalDeck(decks.value[deckIndex])
      }

      saveLocalSnapshot()
      return card
    }

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

    if (!isCloudMode()) {
      const index = cards.value.findIndex((item) => item.id === cardId)
      if (index === -1) return false

      const collectionId = resolveCollectionId(card)
      cards.value[index] = {
        ...cards.value[index],
        collectionId,
        deckId: collectionId,
        front: front.trim(),
        back: back.trim(),
        hint: hint.trim() || undefined,
        tags,
      }

      const deckIndex = decks.value.findIndex((deck) => deck.id === collectionId)
      if (deckIndex !== -1) {
        decks.value[deckIndex] = touchLocalDeck(decks.value[deckIndex])
      }

      saveLocalSnapshot()
      return true
    }

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
    if (!isCloudMode()) {
      const card = cards.value.find((item) => item.id === cardId)
      cards.value = cards.value.filter((item) => item.id !== cardId)

      if (card) {
        const deckId = resolveCollectionId(card)
        const deckIndex = decks.value.findIndex((deck) => deck.id === deckId)
        if (deckIndex !== -1) {
          decks.value[deckIndex] = touchLocalDeck(decks.value[deckIndex])
        }
      }

      saveLocalSnapshot()
      return
    }

    await StudyAPI.deleteCard(cardId)
    cards.value = cards.value.filter((item) => item.id !== cardId)
    await syncDecks()
  }

  async function reviewCard(cardId: string, difficulty: CardDifficulty) {
    if (!isCloudMode()) {
      throw new Error('Review API requires authentication.')
    }

    const index = cards.value.findIndex((item) => item.id === cardId)
    if (index === -1) return false

    const response = await StudyAPI.reviewCard(cardId, difficulty)
    cards.value[index] = response.data
    await syncDecks()
    return true
  }

  async function importCardsFromText(deckId: string, rawText: string): Promise<ImportCardsResult> {
    if (!isCloudMode()) {
      const lines = rawText.split(/\r?\n/)
      let imported = 0
      let skipped = 0

      for (const rawLine of lines) {
        const line = rawLine.trim()
        if (!line) {
          continue
        }

        const separatorIndex = line.indexOf(':')
        if (separatorIndex <= 0 || separatorIndex === line.length - 1) {
          skipped += 1
          continue
        }

        const front = line.slice(0, separatorIndex).trim()
        const back = line.slice(separatorIndex + 1).trim()
        if (!front || !back) {
          skipped += 1
          continue
        }

        cards.value.push(makeLocalCard({
          collectionId: deckId,
          front,
          back,
          hint: '',
          tags: [],
        }))
        imported += 1
      }

      const deckIndex = decks.value.findIndex((deck) => deck.id === deckId)
      if (deckIndex !== -1) {
        decks.value[deckIndex] = touchLocalDeck(decks.value[deckIndex])
      }

      saveLocalSnapshot()
      return { imported, skipped }
    }

    const response = await StudyAPI.importCards(deckId, rawText)
    const cardsResponse = await StudyAPI.getCards(deckId)
    const otherCards = cards.value.filter((card) => resolveCollectionId(card) !== deckId)
    cards.value = [...otherCards, ...cardsResponse.data]
    await syncDecks()
    return response.data
  }

  async function checkDeepAnswer(cardId: string, answer: string): Promise<DeepStudyAnswerResult> {
    if (!isCloudMode()) {
      throw new Error('Deep answer API requires authentication.')
    }

    const response = await StudyAPI.checkDeepAnswer(cardId, answer)
    return response.data
  }

  async function syncDecks() {
    if (!isCloudMode()) {
      saveLocalSnapshot()
      return
    }

    const response = await StudyAPI.getDecks()
    decks.value = response.data.map((deck) => normalizeRemoteDeck(deck)).filter((deck) => Boolean(deck.id))
    lastLoadSource.value = 'cloud'
  }

  return {
    decks,
    cards,
    lastLoadSource,
    deckList,
    load,
    loadForLearn,
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
