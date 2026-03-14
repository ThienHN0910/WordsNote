<template>
  <div class="session-page container py-4">
    <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-4">
      <div>
        <h1 class="h3 mb-1">Luyen tap: {{ deck?.title }}</h1>
        <p class="text-muted mb-0">Hoc theo flashcard hoac quiz va cap nhat lich on theo do kho.</p>
      </div>
      <button class="btn btn-outline-secondary" @click="goBack">Quay lai quan ly</button>
    </div>

    <div v-if="!deck" class="alert alert-warning">Khong tim thay collection.</div>

    <template v-else>
      <div class="d-flex gap-2 flex-wrap mb-3">
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
          Quiz
        </button>
      </div>

      <div v-if="studyCards.length === 0" class="alert alert-info">
        Collection nay chua co card den han on. Ban co the them card moi hoac doi lai lich on.
      </div>

      <section v-else class="study-card p-4">
        <div class="small text-muted mb-3">Card {{ currentIndex + 1 }} / {{ studyCards.length }}</div>

        <div v-if="mode === 'flash'">
          <h2 class="h4 mb-2">{{ currentCard.front }}</h2>
          <p v-if="showAnswer" class="lead mb-2">{{ currentCard.back }}</p>
          <p v-else class="text-muted mb-2">Nhan "Xem dap an" de hien thi mat sau.</p>
          <p v-if="currentCard.hint" class="small fst-italic text-muted">Hint: {{ currentCard.hint }}</p>

          <div class="d-flex gap-2 flex-wrap mt-3">
            <button v-if="!showAnswer" class="btn btn-outline-dark" @click="showAnswer = true">Xem dap an</button>
            <template v-else>
              <button class="btn btn-outline-danger" @click="gradeAndNext('hard')">Kho</button>
              <button class="btn btn-outline-warning" @click="gradeAndNext('medium')">Trung binh</button>
              <button class="btn btn-outline-success" @click="gradeAndNext('easy')">De</button>
            </template>
          </div>
        </div>

        <div v-else>
          <h2 class="h4 mb-2">{{ currentCard.front }}</h2>
          <p class="text-muted">Tu dien nghia hoac cau tra loi cua ban:</p>
          <input
            v-model="quizAnswer"
            class="form-control mb-3"
            type="text"
            placeholder="Nhap dap an"
            @keyup.enter="submitQuiz"
          />

          <div class="d-flex gap-2 flex-wrap mb-3">
            <button class="btn btn-primary" @click="submitQuiz">Cham diem</button>
            <button class="btn btn-outline-secondary" @click="skipQuiz">Bo qua</button>
          </div>

          <div v-if="quizFeedback" class="alert" :class="quizCorrect ? 'alert-success' : 'alert-warning'">
            <div class="fw-semibold">{{ quizFeedback }}</div>
            <div>Dap an: {{ currentCard.back }}</div>
          </div>
        </div>
      </section>
    </template>
  </div>
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

function normalizeText(text: string) {
  return text.trim().toLowerCase()
}

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

  const expected = normalizeText(currentCard.value.back)
  const submitted = normalizeText(quizAnswer.value)
  const correct = submitted.length > 0 && (submitted.includes(expected) || expected.includes(submitted))

  quizCorrect.value = correct
  quizFeedback.value = correct ? 'Dung roi! Card da duoc day lich on xa hon.' : 'Chua chinh xac. Ban nen gap lai card nay som hon.'

  await gradeAndNext(correct ? 'easy' : 'hard')
}

async function skipQuiz() {
  if (!currentCard.value) return

  quizCorrect.value = false
  quizFeedback.value = 'Da bo qua card nay.'
  await gradeAndNext('hard')
}

function goBack() {
  router.push({ name: 'studyHub' })
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
.session-page {
  max-width: 900px;
}

.study-card {
  border: 1px solid #e6e9ef;
  border-radius: 1rem;
  background: linear-gradient(145deg, #ffffff, #f8fbff);
}
</style>
