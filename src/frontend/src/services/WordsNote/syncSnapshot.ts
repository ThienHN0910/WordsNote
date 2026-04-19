import type { StudyCard, StudyDeck } from '@/types/WordsNote'

function normalizeSyncText(value: string) {
  return String(value || '').trim().toLowerCase()
}

function normalizeTags(tags: string[]) {
  return [...tags].map((tag) => normalizeSyncText(tag)).filter(Boolean).sort().join('|')
}

export function createDeckMatchKey(title: string) {
  return normalizeSyncText(title)
}

export function createDeckFingerprint(deck: Pick<StudyDeck, 'title' | 'description'>) {
  return [
    createDeckMatchKey(deck.title),
    normalizeSyncText(deck.description),
  ].join('::')
}

export function createCardMatchKey(collectionId: string, front: string, back: string) {
  return [
    normalizeSyncText(collectionId),
    normalizeSyncText(front),
    normalizeSyncText(back),
  ].join('::')
}

export function createCardFingerprint(card: Pick<StudyCard, 'collectionId' | 'front' | 'back' | 'hint' | 'tags'>) {
  return [
    createCardMatchKey(card.collectionId, card.front, card.back),
    normalizeSyncText(card.hint ?? ''),
    normalizeTags(card.tags),
  ].join('::')
}
