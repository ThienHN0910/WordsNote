import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { QuizAPI, type QuizQuestionsQueryParams } from '@/apis/WordsNote/QuizAPI'
import type { QuizQuestion, QuizSet, QuizSubmitResult } from '@/types/WordsNote'

export const useQuizStore = defineStore('quizStore', () => {
  const quizSets = ref<QuizSet[]>([])
  const currentQuizSet = ref<QuizSet | null>(null)
  const questions = ref<QuizQuestion[]>([])
  const currentQuestionIndex = ref(0)
  const userAnswers = ref<Record<string, string[]>>({})
  const bookmarkedQuestionIds = ref<string[]>([])
  const isSubmitted = ref(false)
  const submitResult = ref<QuizSubmitResult | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const currentQuestion = computed<QuizQuestion | null>(() => {
    if (questions.value.length === 0) return null
    return questions.value[currentQuestionIndex.value] || null
  })

  const totalQuestions = computed(() => questions.value.length)
  const answeredCount = computed(() => Object.keys(userAnswers.value).length)

  const progressPercentage = computed(() => {
    if (totalQuestions.value === 0) return 0
    return Math.round((answeredCount.value / totalQuestions.value) * 100)
  })

  async function fetchQuizSets() {
    isLoading.value = true
    error.value = null
    try {
      const response = await QuizAPI.getQuizSets()
      quizSets.value = response.data
    } catch (err: any) {
      error.value = err?.response?.data?.error || 'Failed to load quiz sets'
    } finally {
      isLoading.value = false
    }
  }

  async function loadQuestions(subjectId: string, queryParams?: QuizQuestionsQueryParams) {
    isLoading.value = true
    error.value = null
    isSubmitted.value = false
    submitResult.value = null
    userAnswers.value = {}
    currentQuestionIndex.value = 0

    try {
      const setResponse = await QuizAPI.getQuizSet(subjectId)
      currentQuizSet.value = setResponse.data

      const questionsResponse = await QuizAPI.getQuestions(subjectId, queryParams)
      questions.value = questionsResponse.data
    } catch (err: any) {
      error.value = err?.response?.data?.error || 'Failed to load questions'
    } finally {
      isLoading.value = false
    }
  }

  function selectAnswer(questionId: string, optionKey: string, isSingleSelect: boolean = true) {
    if (isSubmitted.value) return

    const current = userAnswers.value[questionId] || []
    if (isSingleSelect) {
      userAnswers.value = {
        ...userAnswers.value,
        [questionId]: [optionKey],
      }
    } else {
      const exists = current.includes(optionKey)
      const updated = exists ? current.filter((k) => k !== optionKey) : [...current, optionKey]
      userAnswers.value = {
        ...userAnswers.value,
        [questionId]: updated,
      }
    }
  }

  function toggleBookmark(questionId: string) {
    if (bookmarkedQuestionIds.value.includes(questionId)) {
      bookmarkedQuestionIds.value = bookmarkedQuestionIds.value.filter((id) => id !== questionId)
    } else {
      bookmarkedQuestionIds.value.push(questionId)
    }
  }

  async function submitCurrentQuiz() {
    if (!currentQuizSet.value || questions.value.length === 0) return

    isLoading.value = true
    error.value = null
    try {
      const response = await QuizAPI.submitQuiz(currentQuizSet.value.id, userAnswers.value)
      submitResult.value = response.data
      isSubmitted.value = true
    } catch (err: any) {
      error.value = err?.response?.data?.error || 'Failed to submit quiz'
    } finally {
      isLoading.value = false
    }
  }

  function goToQuestion(index: number) {
    if (index >= 0 && index < questions.value.length) {
      currentQuestionIndex.value = index
    }
  }

  function nextQuestion() {
    if (currentQuestionIndex.value < questions.value.length - 1) {
      currentQuestionIndex.value++
    }
  }

  function prevQuestion() {
    if (currentQuestionIndex.value > 0) {
      currentQuestionIndex.value--
    }
  }

  function resetQuiz() {
    userAnswers.value = {}
    isSubmitted.value = false
    submitResult.value = null
    currentQuestionIndex.value = 0
  }

  return {
    quizSets,
    currentQuizSet,
    questions,
    currentQuestionIndex,
    currentQuestion,
    userAnswers,
    bookmarkedQuestionIds,
    isSubmitted,
    submitResult,
    isLoading,
    error,
    totalQuestions,
    answeredCount,
    progressPercentage,
    fetchQuizSets,
    loadQuestions,
    selectAnswer,
    toggleBookmark,
    submitCurrentQuiz,
    goToQuestion,
    nextQuestion,
    prevQuestion,
    resetQuiz,
  }
})
