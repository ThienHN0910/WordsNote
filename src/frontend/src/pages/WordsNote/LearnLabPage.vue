<template>
  <section class="learn-wrap">
    <header class="learn-header">
      <p class="eyebrow">Dynamic Learning Space</p>
      <h1>Flashcards, Learn, Practice from your Manage collections.</h1>
      <p>
        Learn uses public cloud collections when available and automatically falls back to local
        <RouterLink to="/manage">Manage</RouterLink> data.
      </p>
      <p v-if="!isLoadingManage" class="source-note ok">{{ sourceNote }}</p>
      <p v-else class="source-note">Loading collection data...</p>
      <p v-if="manageLoadError" class="source-note warn">{{ manageLoadError }}</p>
    </header>

    <div class="learn-grid">
      <aside class="deck-panel">
        <h2>Decks</h2>
        <p v-if="isLoadingManage" class="muted">Loading your Manage decks...</p>
        <p v-else-if="learnDecks.length === 0" class="muted">
          No collections yet. Create one in <RouterLink class="manage-link" to="/manage">Manage</RouterLink>.
        </p>

        <button
          v-for="deck in learnDecks"
          :key="deck.id"
          class="deck-button"
          :class="{ active: activeDeck?.id === deck.id }"
          @click="selectDeck(deck.id)"
        >
          <strong>{{ deck.title }}</strong>
          <small>{{ deck.cards.length }} cards</small>
        </button>
      </aside>

      <main class="lab-panel">
        <div v-if="!activeDeck" class="empty-state">
          <p>No deck available yet.</p>
          <RouterLink class="manage-link" to="/manage">Create collection in Manage</RouterLink>
        </div>

        <template v-else>
          <div class="mode-switch">
            <button :class="{ active: mode === 'flash' }" @click="mode = 'flash'">Flashcards</button>
            <button :class="{ active: mode === 'learn' }" @click="mode = 'learn'">Learn</button>
            <button :class="{ active: mode === 'practice' }" @click="mode = 'practice'">Practice</button>
          </div>

          <p class="deck-title">{{ activeDeck.title }} · {{ activeDeck.description }}</p>

          <section class="card-stage">
            <template v-if="!currentCard">
              <p class="muted">This deck has no cards yet.</p>
              <RouterLink class="manage-link" to="/manage">Add cards in Manage</RouterLink>
            </template>

            <template v-else-if="mode === 'flash'">
              <p class="flash-progress">Card {{ currentIndex + 1 }} / {{ activeDeck.cards.length }}</p>

              <div class="flip-card" :class="{ flipped: showBack }" @click="showBack = !showBack">
                <div class="flip-card-inner">
                  <article class="flip-face flip-front">
                    <p class="face-label">Front</p>
                    <h3>{{ currentCard.front }}</h3>
                    <small v-if="currentCard.hint">Hint: {{ currentCard.hint }}</small>
                  </article>
                  <article class="flip-face flip-back">
                    <p class="face-label">Back</p>
                    <h3>{{ currentCard.back }}</h3>
                    <small v-if="currentCard.hint">Hint: {{ currentCard.hint }}</small>
                  </article>
                </div>
              </div>

              <div class="actions">
                <button class="ghost" @click="prevCard">Previous</button>
                <button class="soft" @click="showBack = !showBack">{{ showBack ? 'Show front' : 'Flip card' }}</button>
                <button @click="nextCard">Next</button>
              </div>
            </template>

            <template v-else-if="mode === 'learn'">
              <h3>{{ currentCard.front }}</h3>
              <input v-model="typedAnswer" type="text" placeholder="Type your answer" @keyup.enter="checkLearn" />
              <div class="actions">
                <button @click="checkLearn">{{ learnAwaitingNext ? 'Next card' : 'Check' }}</button>
                <button class="soft" @click="nextCard">Skip</button>
              </div>
              <p v-if="learnFeedback" :class="learnIsCorrect ? 'ok' : 'warn'">{{ learnFeedback }}</p>
            </template>

            <template v-else>
              <h3>{{ practiceQuestion.prompt }}</h3>
              <div class="option-grid">
                <button
                  v-for="option in practiceQuestion.options"
                  :key="option"
                  class="option"
                  :class="{
                    selected: selectedPracticeOption === option,
                    correct: practiceAwaitingNext && option === practiceQuestion.answer,
                    wrong:
                      practiceAwaitingNext &&
                      selectedPracticeOption === option &&
                      option !== practiceQuestion.answer,
                  }"
                  :disabled="practiceAwaitingNext"
                  @click="answerPractice(option)"
                >
                  {{ option }}
                </button>
              </div>

              <div class="actions" v-if="practiceAwaitingNext">
                <button @click="nextCard">Next question</button>
              </div>

              <p v-if="practiceFeedback" :class="practiceFeedbackType">{{ practiceFeedback }}</p>
            </template>
          </section>
        </template>
      </main>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { useStudyStore } from '@/stores/WordsNote/StudyStore'

interface LearnCard {
  id: string
  front: string
  back: string
  hint?: string
  tags: string[]
}

interface LearnDeck {
  id: string
  title: string
  description: string
  cards: LearnCard[]
}

const studyStore = useStudyStore()

const activeDeckId = ref('')
const mode = ref<'flash' | 'learn' | 'practice'>('flash')
const currentIndex = ref(0)
const showBack = ref(false)

const typedAnswer = ref('')
const learnFeedback = ref('')
const learnIsCorrect = ref(false)
const learnAwaitingNext = ref(false)

const selectedPracticeOption = ref('')
const practiceFeedback = ref('')
const practiceFeedbackType = ref('')
const practiceAwaitingNext = ref(false)
const practiceOptions = ref<string[]>([])

const isLoadingManage = ref(false)
const manageLoadError = ref('')
let learnAutoAdvanceTimer: ReturnType<typeof setTimeout> | null = null

const dynamicDecks = computed<LearnDeck[]>(() =>
  studyStore.deckList.map((deck) => ({
    id: deck.id,
    title: deck.title,
    description: deck.description || 'No description',
    cards: studyStore.getDeckCards(deck.id).map((card) => ({
      id: card.id,
      front: card.front,
      back: card.back,
      hint: card.hint,
      tags: card.tags,
    })),
  })),
)

const learnDecks = computed(() => dynamicDecks.value)
const activeDeck = computed(() => learnDecks.value.find((deck) => deck.id === activeDeckId.value) ?? null)
const sourceNote = computed(() => {
  if (studyStore.lastLoadSource === 'cloud') {
    return 'Showing live public collections from cloud.'
  }

  return 'Cloud source unavailable. Showing local collections from browser storage.'
})

const currentCard = computed(() => {
  if (!activeDeck.value || activeDeck.value.cards.length === 0) {
    return null
  }

  const normalizedIndex = currentIndex.value % activeDeck.value.cards.length
  return activeDeck.value.cards[normalizedIndex] ?? null
})

const practiceQuestion = computed(() => {
  if (!currentCard.value) {
    return {
      prompt: '',
      answer: '',
      options: [] as string[],
    }
  }

  return {
    prompt: currentCard.value.front,
    answer: currentCard.value.back,
    options: practiceOptions.value,
  }
})

function shuffle(values: string[]) {
  const output = [...values]
  for (let i = output.length - 1; i > 0; i -= 1) {
    const j = Math.floor(Math.random() * (i + 1))
    const temp = output[i]
    output[i] = output[j]
    output[j] = temp
  }
  return output
}

function clearLearnAutoAdvanceTimer() {
  if (learnAutoAdvanceTimer) {
    clearTimeout(learnAutoAdvanceTimer)
    learnAutoAdvanceTimer = null
  }
}

function rebuildPracticeOptions() {
  if (!currentCard.value || !activeDeck.value) {
    practiceOptions.value = []
    return
  }

  const distractors = activeDeck.value.cards
    .filter((card) => card.id !== currentCard.value?.id)
    .map((card) => card.back)
    .slice(0, 3)

  const options = [...new Set([currentCard.value.back, ...distractors])]
  practiceOptions.value = shuffle(options)
}

function normalize(value: string) {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim()
    .replace(/\s+/g, ' ')
}

function resetLearningState() {
  clearLearnAutoAdvanceTimer()
  showBack.value = false

  typedAnswer.value = ''
  learnFeedback.value = ''
  learnIsCorrect.value = false
  learnAwaitingNext.value = false

  selectedPracticeOption.value = ''
  practiceFeedback.value = ''
  practiceFeedbackType.value = ''
  practiceAwaitingNext.value = false
}

function nextCard() {
  if (!activeDeck.value || activeDeck.value.cards.length === 0) return
  resetLearningState()
  currentIndex.value = (currentIndex.value + 1) % activeDeck.value.cards.length
}

function prevCard() {
  if (!activeDeck.value || activeDeck.value.cards.length === 0) return
  resetLearningState()
  const totalCards = activeDeck.value.cards.length
  currentIndex.value = (currentIndex.value - 1 + totalCards) % totalCards
}

function selectDeck(deckId: string) {
  activeDeckId.value = deckId
  currentIndex.value = 0
  resetLearningState()
}

function checkLearn() {
  if (!currentCard.value) return

  if (learnAwaitingNext.value) {
    nextCard()
    return
  }

  const isCorrect = normalize(typedAnswer.value) === normalize(currentCard.value.back)
  learnIsCorrect.value = isCorrect
  clearLearnAutoAdvanceTimer()

  if (isCorrect) {
    learnAwaitingNext.value = false
    learnFeedback.value = 'Correct. Moving to the next card...'
    const answeredCardId = currentCard.value.id
    learnAutoAdvanceTimer = setTimeout(() => {
      if (mode.value !== 'learn') return
      if (currentCard.value?.id !== answeredCardId) return
      nextCard()
    }, 1000)
    return
  }

  learnAwaitingNext.value = true
  learnFeedback.value = `Not yet. Expected answer: ${currentCard.value.back}. Press Enter again for next card.`
}

function answerPractice(option: string) {
  if (!currentCard.value || practiceAwaitingNext.value) return

  const isCorrect = option === practiceQuestion.value.answer
  selectedPracticeOption.value = option
  practiceAwaitingNext.value = true
  practiceFeedbackType.value = isCorrect ? 'ok' : 'warn'
  practiceFeedback.value = isCorrect
    ? 'Correct. Nicely done.'
    : `Wrong. Correct answer: ${practiceQuestion.value.answer}`
}

watch(
  learnDecks,
  (decks) => {
    if (decks.length === 0) {
      activeDeckId.value = ''
      return
    }

    const hasCurrent = decks.some((deck) => deck.id === activeDeckId.value)
    if (!hasCurrent) {
      activeDeckId.value = decks[0].id
      currentIndex.value = 0
      resetLearningState()
    }
  },
  { immediate: true },
)

watch(mode, () => {
  resetLearningState()
})

watch(
  () => currentCard.value?.id,
  () => {
    rebuildPracticeOptions()
  },
  { immediate: true },
)

async function loadManageDecks() {
  isLoadingManage.value = true
  manageLoadError.value = ''

  try {
    await studyStore.loadForLearn()
  } catch {
    manageLoadError.value = 'Could not load Manage collections. Please try again.'
  } finally {
    isLoadingManage.value = false
  }
}

onMounted(async () => {
  await loadManageDecks()
})

onUnmounted(() => {
  clearLearnAutoAdvanceTimer()
})
</script>

<style scoped>
.learn-wrap {
  max-width: 1120px;
  margin: 0 auto;
  padding: 2rem 1rem 3rem;
}

.learn-header {
  margin-bottom: 1rem;
}

.eyebrow {
  text-transform: uppercase;
  letter-spacing: 0.16em;
  font-size: 0.72rem;
  color: var(--wn-muted);
  margin: 0 0 0.35rem;
}

.learn-header h1 {
  margin: 0;
  line-height: 1.12;
}

.learn-header p {
  color: var(--wn-muted);
}

.source-note {
  margin: 0.2rem 0 0;
  font-size: 0.9rem;
}

.learn-grid {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 1rem;
}

.deck-panel,
.lab-panel {
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 20px;
  padding: 1rem;
  box-shadow: var(--wn-shadow-soft);
}

.deck-panel h2 {
  margin-top: 0;
}

.deck-button {
  width: 100%;
  margin-top: 0.5rem;
  text-align: left;
  border-radius: 12px;
  border: 1px solid var(--wn-border);
  background: var(--wn-surface-soft);
  padding: 0.65rem 0.7rem;
  display: flex;
  flex-direction: column;
}

.deck-button small {
  color: var(--wn-muted);
}

.deck-button.active {
  border-color: var(--wn-primary);
  background: var(--wn-primary-soft);
}

.mode-switch {
  display: flex;
  gap: 0.45rem;
  flex-wrap: wrap;
}

.mode-switch button {
  border: 1px solid var(--wn-border);
  background: var(--wn-surface);
  border-radius: 999px;
  padding: 0.42rem 0.85rem;
  color: var(--wn-ink);
}

.mode-switch button.active {
  background: var(--wn-primary);
  color: var(--wn-on-primary);
  border-color: var(--wn-primary);
}

.deck-title {
  margin: 0.8rem 0 0.4rem;
  color: var(--wn-muted);
}

.card-stage {
  border-radius: 16px;
  border: 1px dashed var(--wn-border);
  padding: 1rem;
  background: var(--wn-surface-soft);
  min-height: 270px;
}

.flip-card {
  perspective: 1000px;
  min-height: 190px;
  cursor: pointer;
}

.flip-card-inner {
  position: relative;
  width: 100%;
  min-height: 190px;
  transition: transform 0.5s ease;
  transform-style: preserve-3d;
}

.flip-card.flipped .flip-card-inner {
  transform: rotateY(180deg);
}

.flip-face {
  position: absolute;
  width: 100%;
  min-height: 190px;
  border: 1px solid var(--wn-border);
  border-radius: 14px;
  padding: 1rem;
  display: grid;
  align-content: center;
  gap: 0.45rem;
  backface-visibility: hidden;
  background: var(--wn-surface);
}

.flip-front {
  background: linear-gradient(180deg, var(--wn-surface), var(--wn-surface-soft));
}

.flip-back {
  transform: rotateY(180deg);
  background: linear-gradient(180deg, var(--wn-surface), var(--wn-primary-soft));
}

.face-label {
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  color: var(--wn-muted);
  font-size: 0.72rem;
}

.flash-progress {
  margin: 0 0 0.55rem;
  font-size: 0.86rem;
  color: var(--wn-muted);
  font-weight: 600;
}

.empty-state {
  color: var(--wn-muted);
}

.empty-state p {
  margin-bottom: 0.4rem;
}

.answer {
  font-size: 1.1rem;
  font-weight: 600;
}

.muted {
  color: var(--wn-muted);
}

.manage-link {
  color: var(--wn-primary);
  font-weight: 600;
}

.actions {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.75rem;
  flex-wrap: wrap;
}

.actions button,
.option {
  border: 1px solid var(--wn-border);
  background: var(--wn-surface);
  border-radius: 10px;
  padding: 0.48rem 0.8rem;
  color: var(--wn-ink);
}

.actions button:not(.soft):not(.ghost) {
  background: var(--wn-primary);
  color: var(--wn-on-primary);
  border-color: var(--wn-primary);
}

.actions .ghost {
  background: color-mix(in srgb, var(--wn-accent) 18%, var(--wn-surface));
  border-color: color-mix(in srgb, var(--wn-accent) 50%, var(--wn-border));
  color: var(--wn-ink);
}

.option-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 0.45rem;
  margin-top: 0.55rem;
}

.option.selected {
  border-color: var(--wn-primary);
  background: var(--wn-primary-soft);
}

.option.correct {
  border-color: #0f766e;
  background: #e6fffa;
}

.option.wrong {
  border-color: #b45309;
  background: #fff3e6;
}

.option:disabled {
  opacity: 1;
  cursor: default;
}

.ok {
  color: #0f766e;
  font-weight: 600;
}

.warn {
  color: #b45309;
  font-weight: 600;
}

@media (max-width: 960px) {
  .learn-grid {
    grid-template-columns: 1fr;
  }
}
</style>
