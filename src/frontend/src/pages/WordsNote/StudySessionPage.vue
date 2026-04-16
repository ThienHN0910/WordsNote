<template>
  <section class="session-wrap">
    <div class="session-head">
      <div>
        <p class="eyebrow">Focused Session</p>
        <h1>{{ deck?.title || 'Collection not found' }}</h1>
        <p>Study with flashcards or type answers in learn mode.</p>
      </div>
      <button class="btn btn-outline-secondary" @click="goBack">Back to Manage</button>
    </div>

    <div v-if="!deck" class="alert alert-warning">Collection not found.</div>

    <template v-else>
      <div class="mode-switch">
        <button
          class="btn"
          :class="mode === 'flash' ? 'btn-primary' : 'btn-outline-primary'"
          @click="mode = 'flash'"
        >
          Flashcard
        </button>
        <button
          class="btn"
          :class="mode === 'quiz' ? 'btn-primary' : 'btn-outline-primary'"
          @click="mode = 'quiz'"
        >
          Learn
        </button>
      </div>

      <div v-if="studyCards.length === 0" class="alert alert-info">
        This collection has no due cards yet. Add more cards or wait for the next review window.
      </div>

      <section v-else class="study-card">
        <div class="small text-muted mb-3">Card {{ currentIndex + 1 }} / {{ studyCards.length }}</div>

        <div v-if="mode === 'flash'">
          <h2 class="h4 mb-2">{{ currentCard.front }}</h2>
          <p v-if="showAnswer" class="lead mb-2">{{ currentCard.back }}</p>
          <p v-else class="text-muted mb-2">Click reveal to show the answer.</p>
          <p v-if="currentCard.hint" class="small fst-italic text-muted">Hint: {{ currentCard.hint }}</p>

          <div class="d-flex gap-2 flex-wrap mt-3">
            <button v-if="!showAnswer" class="btn btn-outline-dark" @click="showAnswer = true">Reveal</button>
            <template v-else>
              <button class="btn btn-outline-danger" @click="gradeAndNext('hard')">Hard</button>
              <button class="btn btn-outline-warning" @click="gradeAndNext('medium')">Medium</button>
              <button class="btn btn-outline-success" @click="gradeAndNext('easy')">Easy</button>
            </template>
          </div>
        </div>

        <div v-else>
          <h2 class="h4 mb-2">{{ currentCard.front }}</h2>
          <p class="text-muted">Type your answer:</p>
          <input
            v-model="quizAnswer"
            class="form-control mb-3"
            type="text"
            placeholder="Enter answer"
            @keyup.enter="submitQuiz"
          />

          <div class="d-flex gap-2 flex-wrap mb-3">
            <button class="btn btn-primary" @click="submitQuiz">Check answer</button>
            <button class="btn btn-outline-secondary" @click="skipQuiz">Skip</button>
          </div>

          <div v-if="quizFeedback" class="alert" :class="quizCorrect ? 'alert-success' : 'alert-warning'">
            <div class="fw-semibold">{{ quizFeedback }}</div>
            <div>Expected answer: {{ currentCard.back }}</div>
          </div>
        </div>
      </section>
    </template>
  </section>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useStudyStore } from '@/stores/WordsNote/StudyStore'
import type { CardDifficulty } from '@/types/WordsNote'

const route = useRoute()
const router = useRouter()
const studyStore = useStudyStore()

const mode = ref<'flash' | 'quiz'>('flash')
const currentIndex = ref(0)
const showAnswer = ref(false)
const quizAnswer = ref('')
const quizFeedback = ref('')
const quizCorrect = ref(false)

const deckId = computed(() => String(route.params.deckId ?? ''))
const deck = computed(() => studyStore.getDeckById(deckId.value))
const studyCards = computed(() => {
  const dueCards = studyStore.getDueCards(deckId.value)
  if (dueCards.length > 0) {
    return dueCards
  }

  return studyStore.getDeckCards(deckId.value)
})
const currentCard = computed(() => studyCards.value[currentIndex.value])

function nextCard() {
  showAnswer.value = false
  quizAnswer.value = ''
  quizFeedback.value = ''

  if (studyCards.value.length === 0) {
    currentIndex.value = 0
    return
  }

  currentIndex.value = (currentIndex.value + 1) % studyCards.value.length
}

async function gradeAndNext(difficulty: CardDifficulty) {
  if (!currentCard.value) return

  await studyStore.reviewCard(currentCard.value.id, difficulty)
  nextCard()
}

async function submitQuiz() {
  if (!currentCard.value) return

  const result = await studyStore.checkDeepAnswer(currentCard.value.id, quizAnswer.value)
  const correct = result.isCorrect
  const difficulty = result.recommendedDifficulty === 'medium'
    ? 'medium'
    : result.recommendedDifficulty === 'easy'
      ? 'easy'
      : 'hard'

  quizCorrect.value = correct
  quizFeedback.value = correct
    ? 'Correct. This card will appear less often.'
    : 'Not correct yet. This card will come back sooner.'

  await gradeAndNext(difficulty)
}

async function skipQuiz() {
  if (!currentCard.value) return

  quizCorrect.value = false
  quizFeedback.value = 'Skipped. We will revisit this card soon.'
  await gradeAndNext('hard')
}

function goBack() {
  router.push({ name: 'manageCollections' })
}

onMounted(async () => {
  if (studyStore.deckList.length === 0) {
    await studyStore.load()
  }
})

watch(mode, () => {
  showAnswer.value = false
  quizAnswer.value = ''
  quizFeedback.value = ''
})
</script>

<style scoped>
.session-wrap {
  max-width: 960px;
  margin: 0 auto;
  padding: 2rem 1rem 3rem;
}

.session-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 0.8rem;
  flex-wrap: wrap;
  margin-bottom: 1rem;
}

.eyebrow {
  margin: 0;
  text-transform: uppercase;
  font-size: 0.72rem;
  letter-spacing: 0.16em;
  color: var(--wn-muted);
}

.session-head h1 {
  margin: 0.3rem 0;
}

.session-head p {
  margin: 0;
  color: var(--wn-muted);
}

.mode-switch {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.8rem;
}

.study-card {
  border: 1px solid var(--wn-border);
  border-radius: 18px;
  background: linear-gradient(145deg, var(--wn-surface-soft), var(--wn-surface));
  padding: 1.2rem;
  box-shadow: var(--wn-shadow-soft);
}
</style>
