import type { StudyCard, StudyDeck, StudyLocalCloudSyncState, StudySnapshot } from '@/types/WordsNote'

const STORAGE_KEY = 'wordsnote_manage_local_data_v1'
const SYNC_STATE_KEY = 'wordsnote_manage_local_cloud_sync_state_v1'

function getNowIso() {
  return new Date().toISOString()
}

function createLocalId(prefix: 'deck' | 'card') {
  return `local-${prefix}-${Math.random().toString(36).slice(2, 10)}-${Date.now()}`
}

function normalizeDeck(raw: Partial<StudyDeck>): StudyDeck {
  const now = getNowIso()
  return {
    id: String(raw.id || createLocalId('deck')),
    title: String(raw.title || '').trim(),
    description: String(raw.description || '').trim(),
    createdAt: String(raw.createdAt || now),
    updatedAt: String(raw.updatedAt || now),
  }
}

function normalizeCard(raw: Partial<StudyCard>): StudyCard {
  const now = getNowIso()
  return {
    id: String(raw.id || createLocalId('card')),
    collectionId: String(raw.collectionId || raw.deckId || ''),
    deckId: raw.deckId ? String(raw.deckId) : undefined,
    front: String(raw.front || '').trim(),
    back: String(raw.back || '').trim(),
    hint: raw.hint ? String(raw.hint) : undefined,
    tags: Array.isArray(raw.tags) ? raw.tags.map((item) => String(item).trim()).filter(Boolean) : [],
    dueAt: String(raw.dueAt || now),
    lastReviewedAt: raw.lastReviewedAt ? String(raw.lastReviewedAt) : undefined,
    streak: Number(raw.streak || 0),
  }
}

function emptySnapshot(): StudySnapshot {
  return {
    decks: [],
    cards: [],
  }
}

function emptySyncState(): StudyLocalCloudSyncState {
  return {
    deckIdMap: {},
    cardIdMap: {},
  }
}

export function loadLocalStudySnapshot(): StudySnapshot {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return emptySnapshot()
  }

  try {
    const parsed = JSON.parse(raw) as Partial<StudySnapshot>
    const decks = Array.isArray(parsed.decks) ? parsed.decks.map((item) => normalizeDeck(item)) : []
    const cards = Array.isArray(parsed.cards) ? parsed.cards.map((item) => normalizeCard(item)) : []
    return { decks, cards }
  } catch {
    return emptySnapshot()
  }
}

export function saveLocalStudySnapshot(snapshot: StudySnapshot) {
  const payload: StudySnapshot = {
    decks: snapshot.decks.map((item) => normalizeDeck(item)),
    cards: snapshot.cards.map((item) => normalizeCard(item)),
  }

  localStorage.setItem(STORAGE_KEY, JSON.stringify(payload))
}

export function loadLocalCloudSyncState(): StudyLocalCloudSyncState {
  const raw = localStorage.getItem(SYNC_STATE_KEY)
  if (!raw) {
    return emptySyncState()
  }

  try {
    const parsed = JSON.parse(raw) as Partial<StudyLocalCloudSyncState>
    const deckIdMap = parsed.deckIdMap && typeof parsed.deckIdMap === 'object' ? parsed.deckIdMap : {}
    const cardIdMap = parsed.cardIdMap && typeof parsed.cardIdMap === 'object' ? parsed.cardIdMap : {}

    return {
      deckIdMap: Object.fromEntries(
        Object.entries(deckIdMap)
          .map(([localId, cloudId]) => [String(localId).trim(), String(cloudId).trim()])
          .filter(([localId, cloudId]) => Boolean(localId && cloudId)),
      ),
      cardIdMap: Object.fromEntries(
        Object.entries(cardIdMap)
          .map(([localId, cloudId]) => [String(localId).trim(), String(cloudId).trim()])
          .filter(([localId, cloudId]) => Boolean(localId && cloudId)),
      ),
    }
  } catch {
    return emptySyncState()
  }
}

export function saveLocalCloudSyncState(state: StudyLocalCloudSyncState) {
  const payload: StudyLocalCloudSyncState = {
    deckIdMap: Object.fromEntries(
      Object.entries(state.deckIdMap || {})
        .map(([localId, cloudId]) => [String(localId).trim(), String(cloudId).trim()])
        .filter(([localId, cloudId]) => Boolean(localId && cloudId)),
    ),
    cardIdMap: Object.fromEntries(
      Object.entries(state.cardIdMap || {})
        .map(([localId, cloudId]) => [String(localId).trim(), String(cloudId).trim()])
        .filter(([localId, cloudId]) => Boolean(localId && cloudId)),
    ),
  }

  localStorage.setItem(SYNC_STATE_KEY, JSON.stringify(payload))
}

export function makeLocalDeck(title: string, description: string): StudyDeck {
  const now = getNowIso()
  return {
    id: createLocalId('deck'),
    title: title.trim(),
    description: description.trim(),
    createdAt: now,
    updatedAt: now,
  }
}

export function makeLocalCard(payload: {
  collectionId: string
  front: string
  back: string
  hint: string
  tags: string[]
}): StudyCard {
  const now = getNowIso()
  const collectionId = payload.collectionId.trim()

  return {
    id: createLocalId('card'),
    collectionId,
    deckId: collectionId,
    front: payload.front.trim(),
    back: payload.back.trim(),
    hint: payload.hint.trim() || undefined,
    tags: payload.tags,
    dueAt: now,
    streak: 0,
  }
}

export function touchLocalDeck(deck: StudyDeck): StudyDeck {
  return {
    ...deck,
    updatedAt: getNowIso(),
  }
}
