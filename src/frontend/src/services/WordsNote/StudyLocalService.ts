import type {
  StudyCard,
  StudyDeck,
  StudySnapshot,
  CardDifficulty,
  StudyDeckStats,
} from '@/types/WordsNote'

const STORAGE_KEY = 'words-note-study-v1'

function nowIso(): string {
  return new Date().toISOString()
}

function createId(prefix: string): string {
  return `${prefix}-${Math.random().toString(36).slice(2, 10)}`
}

function addDays(dateIso: string, days: number): string {
  const date = new Date(dateIso)
  date.setDate(date.getDate() + days)
  return date.toISOString()
}

const seedDeckId = 'seed-basic-english'

const seedDeck: StudyDeck = {
  id: seedDeckId,
  title: 'Basic English',
  description: 'Bo the co ban de hoc moi ngay',
  createdAt: nowIso(),
  updatedAt: nowIso(),
}

const seedCards: StudyCard[] = [
  {
    id: createId('card'),
    deckId: seedDeckId,
    front: 'abandon',
    back: 'tu bo',
    hint: 'Dong nghia voi leave behind',
    tags: ['ielts'],
    dueAt: nowIso(),
    streak: 0,
  },
  {
    id: createId('card'),
    deckId: seedDeckId,
    front: 'concise',
    back: 'ngan gon, suc tich',
    tags: ['writing'],
    dueAt: nowIso(),
    streak: 0,
  },
  {
    id: createId('card'),
    deckId: seedDeckId,
    front: 'resilient',
    back: 'kien cuong',
    tags: ['daily'],
    dueAt: nowIso(),
    streak: 0,
  },
]

function defaultSnapshot(): StudySnapshot {
  return {
    decks: [seedDeck],
    cards: seedCards,
  }
}

function normalizeSnapshot(snapshot: StudySnapshot): StudySnapshot {
  return {
    decks: Array.isArray(snapshot.decks) ? snapshot.decks : [],
    cards: Array.isArray(snapshot.cards) ? snapshot.cards : [],
  }
}

function nextIntervalDays(difficulty: CardDifficulty, streak: number): number {
  const baseByDifficulty: Record<CardDifficulty, number> = {
    hard: 1,
    medium: 2,
    easy: 4,
  }

  const growth = Math.max(streak - 1, 0)
  return baseByDifficulty[difficulty] + growth
}

export const StudyLocalService = {
  load(): StudySnapshot {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) {
      const snapshot = defaultSnapshot()
      this.save(snapshot)
      return snapshot
    }

    try {
      const parsed = JSON.parse(raw) as StudySnapshot
      const snapshot = normalizeSnapshot(parsed)
      return snapshot
    } catch {
      const snapshot = defaultSnapshot()
      this.save(snapshot)
      return snapshot
    }
  },

  save(snapshot: StudySnapshot) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(snapshot))
  },

  createDeck(payload: Pick<StudyDeck, 'title' | 'description'>): StudyDeck {
    const now = nowIso()
    return {
      id: createId('deck'),
      title: payload.title.trim(),
      description: payload.description.trim(),
      createdAt: now,
      updatedAt: now,
    }
  },

  createCard(payload: Pick<StudyCard, 'deckId' | 'front' | 'back' | 'hint' | 'tags'>): StudyCard {
    return {
      id: createId('card'),
      deckId: payload.deckId,
      front: payload.front.trim(),
      back: payload.back.trim(),
      hint: payload.hint?.trim(),
      tags: payload.tags,
      dueAt: nowIso(),
      streak: 0,
    }
  },

  reviewCard(card: StudyCard, difficulty: CardDifficulty): StudyCard {
    const streak = difficulty === 'hard' ? 0 : card.streak + 1
    const intervalDays = nextIntervalDays(difficulty, streak)

    return {
      ...card,
      streak,
      lastReviewedAt: nowIso(),
      dueAt: addDays(nowIso(), intervalDays),
    }
  },

  getDeckStats(deckId: string, cards: StudyCard[]): StudyDeckStats {
    const now = new Date()
    const deckCards = cards.filter((card) => card.deckId === deckId)
    const dueCards = deckCards.filter((card) => new Date(card.dueAt) <= now).length
    const masteredCards = deckCards.filter((card) => card.streak >= 5).length

    return {
      totalCards: deckCards.length,
      dueCards,
      masteredCards,
    }
  },
}
