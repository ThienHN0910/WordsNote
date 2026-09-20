<template>
  <div class="quiz-container">
    <div class="quiz-hero">
      <div class="hero-badge">Exam Preparation & Question Banks</div>
      <h1 class="hero-title">Practice University Subjects</h1>
      <p class="hero-desc">
        Comprehensive question banks for FPT University subjects including MLN122, PRM393, JFE301, and JIT401.
        Study with instant explanations or simulate real exams.
      </p>
    </div>

    <div v-if="isLoading" class="loading-state">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-3 text-muted">Loading question banks...</p>
    </div>

    <div v-else-if="error" class="alert alert-danger" role="alert">
      {{ error }}
    </div>

    <div v-else class="quiz-grid">
      <div
        v-for="set in quizSets"
        :key="set.id"
        class="quiz-card"
        :style="{ '--subject-color': set.color }"
      >
        <div class="card-top">
          <span class="subject-code">{{ set.code }}</span>
          <span class="question-badge">{{ set.totalQuestions }} questions</span>
        </div>

        <h2 class="subject-title">{{ set.title }}</h2>
        <p class="subject-desc">{{ set.description || 'Curated question bank with detailed explanations.' }}</p>

        <div class="action-row">
          <RouterLink
            :to="{ name: 'quizPractice', params: { id: set.id }, query: { mode: 'practice' } }"
            class="btn btn-outline-custom"
          >
            <i class="fa-solid fa-book-open me-2"></i> Practice Mode
          </RouterLink>

          <RouterLink
            :to="{ name: 'quizPractice', params: { id: set.id }, query: { mode: 'exam' } }"
            class="btn btn-primary-custom"
          >
            <i class="fa-solid fa-stopwatch me-2"></i> Mock Exam (40Q)
          </RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useQuizStore } from '@/stores/WordsNote/QuizStore'

const quizStore = useQuizStore()
const { quizSets, isLoading, error } = storeToRefs(quizStore)

onMounted(async () => {
  await quizStore.fetchQuizSets()
})
</script>

<style scoped>
.quiz-container {
  max-width: 1120px;
  margin: 0 auto;
  padding: 2.5rem 1.25rem 4rem;
}

.quiz-hero {
  text-align: center;
  max-width: 680px;
  margin: 0 auto 3rem;
}

.hero-badge {
  display: inline-block;
  padding: 0.35rem 0.85rem;
  border-radius: 9999px;
  font-size: 0.8rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  background: rgba(37, 99, 235, 0.1);
  color: var(--wn-primary, #2563eb);
  margin-bottom: 1rem;
}

.hero-title {
  font-size: 2.4rem;
  font-weight: 800;
  letter-spacing: -0.03em;
  color: var(--wn-ink, #0f172a);
  margin-bottom: 0.85rem;
}

.hero-desc {
  font-size: 1.05rem;
  line-height: 1.6;
  color: var(--wn-ink-muted, #64748b);
  margin: 0;
}

.quiz-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 1.5rem;
}

.quiz-card {
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 16px;
  padding: 1.75rem;
  display: flex;
  flex-direction: column;
  transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease;
}

.quiz-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 24px -10px rgba(0, 0, 0, 0.08);
  border-color: var(--subject-color);
}

.card-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.subject-code {
  font-size: 1.15rem;
  font-weight: 700;
  color: var(--subject-color);
  background: color-mix(in srgb, var(--subject-color) 12%, transparent);
  padding: 0.3rem 0.75rem;
  border-radius: 8px;
}

.question-badge {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--wn-ink-muted, #64748b);
  background: var(--wn-badge-bg, #f1f5f9);
  padding: 0.25rem 0.65rem;
  border-radius: 9999px;
}

.subject-title {
  font-size: 1.3rem;
  font-weight: 700;
  color: var(--wn-ink, #0f172a);
  margin-bottom: 0.5rem;
}

.subject-desc {
  font-size: 0.95rem;
  color: var(--wn-ink-muted, #64748b);
  line-height: 1.5;
  flex-grow: 1;
  margin-bottom: 1.5rem;
}

.action-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}

.btn-outline-custom {
  border: 1px solid var(--wn-card-border, #cbd5e1);
  background: var(--wn-surface, #ffffff);
  color: var(--wn-ink, #1e293b);
  font-weight: 600;
  border-radius: 10px;
  padding: 0.6rem 0.8rem;
  font-size: 0.9rem;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease;
}

.btn-outline-custom:hover {
  background: var(--wn-badge-bg, #f1f5f9);
  color: var(--wn-ink, #1e293b);
}

.btn-primary-custom {
  background: var(--subject-color);
  color: #ffffff;
  font-weight: 600;
  border: none;
  border-radius: 10px;
  padding: 0.6rem 0.8rem;
  font-size: 0.9rem;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.15s ease, transform 0.1s ease;
}

.btn-primary-custom:hover {
  opacity: 0.92;
  color: #ffffff;
}

.btn-primary-custom:active {
  transform: scale(0.98);
}

.loading-state {
  text-align: center;
  padding: 5rem 0;
}
</style>
