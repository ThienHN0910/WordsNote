export type CardDifficulty = 'easy' | 'medium' | 'hard'

export interface StudyCard {
  id: string
  collectionId: string
  deckId?: string
  front: string
  back: string
  hint?: string
  tags: string[]
  dueAt: string
  lastReviewedAt?: string
  streak: number
}

export interface StudyDeck {
  id: string
  title: string
  description: string
  createdAt: string
  updatedAt: string
}

export interface StudyDeckStats {
  totalCards: number
  dueCards: number
  masteredCards: number
}

export interface StudySnapshot {
  decks: StudyDeck[]
  cards: StudyCard[]
}

export interface StudyLocalCloudSyncState {
  deckIdMap: Record<string, string>
  cardIdMap: Record<string, string>
}

export interface DeepStudyAnswerResult {
  cardId: string
  isCorrect: boolean
  expectedAnswer: string
  submittedAnswer: string
  recommendedDifficulty: CardDifficulty
}
