import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { QuizAPI } from '@/apis/WordsNote/QuizAPI'
import type { QuizQuestion, QuizSet, QuizUser } from '@/types/WordsNote'

export type QuizMode = 'seq' | 'wrong' | 'unanswered' | 'starred' | 'shuffle'
export type SourceFilter = 'all' | 'exam' | 'slides' | 'quiz_pt'

export interface QuestionProgress {
  result?: 'ok' | 'bad'
  selected?: string[]
  star?: boolean
  answeredAt?: string
}

export const useQuizStore = defineStore('quizStore', () => {
  // Subjects / Catalog
  const subjects = ref<QuizSet[]>([])
  const activeSubjectId = ref<string>('mln122')
  const loadingData = ref<boolean>(false)
  const isLocked = ref<boolean>(false)
  const error = ref<string | null>(null)

  // Questions & Queue
  const questions = ref<QuizQuestion[]>([])
  const queue = ref<QuizQuestion[]>([])
  const pos = ref<number>(0)
  const currentMode = ref<QuizMode>('seq')
  const currentSource = ref<SourceFilter>('all')

  // Interactive Question State
  const selectedKeys = ref<string[]>([])
  const isChecked = ref<boolean>(false)
  const isRevealed = ref<boolean>(false)

  // Progress Map per Subject: { [qId]: QuestionProgress }
  const progress = ref<Record<string | number, QuestionProgress>>({})

  // User State
  const token = ref<string>(localStorage.getItem('fe_learn_token') || localStorage.getItem('access_token') || '')
  const user = ref<QuizUser | null>(() => {
    try {
      const raw = localStorage.getItem('fe_learn_user')
      return raw ? JSON.parse(raw) : null
    } catch {
      return null
    }
  })

  // Computed Properties
  const currentSubject = computed(() => {
    return subjects.value.find((s) => s.id === activeSubjectId.value) || null
  })

  const currentQuestion = computed<QuizQuestion | null>(() => {
    if (!queue.value.length || pos.value >= queue.value.length) return null
    return queue.value[pos.value] || null
  })

  const stats = computed(() => {
    let ok = 0
    let bad = 0
    const total = questions.value.length

    for (const q of questions.value) {
      const p = progress.value[q.id] || progress.value[q.questionNumber]
      if (p?.result === 'ok') ok++
      else if (p?.result === 'bad') bad++
    }

    const done = ok + bad
    const left = Math.max(0, total - done)
    return { done, ok, bad, left, total }
  })

  const modeLabel = computed(() => {
    const labels: Record<QuizMode, string> = {
      seq: 'Tất cả câu hỏi',
      shuffle: 'Ngẫu nhiên',
      unanswered: 'Chưa làm',
      wrong: 'Làm sai',
      starred: 'Đã đánh dấu sao'
    }
    return labels[currentMode.value] || 'Thứ tự'
  })

  // Storage Helpers
  function loadLocalProgress(subId: string) {
    try {
      const raw = localStorage.getItem(`fe_learn_progress_${subId}`)
      progress.value = raw ? JSON.parse(raw) : {}
    } catch {
      progress.value = {}
    }
  }

  function saveLocalProgress() {
    try {
      localStorage.setItem(`fe_learn_progress_${activeSubjectId.value}`, JSON.stringify(progress.value))
    } catch (e) {
      console.error('Failed to save progress to localStorage:', e)
    }
  }

  function loadCurrentQuestionState() {
    const q = currentQuestion.value
    if (!q) {
      selectedKeys.value = []
      isChecked.value = false
      isRevealed.value = false
      return
    }

    const p = progress.value[q.id] || progress.value[q.questionNumber]
    if (p) {
      selectedKeys.value = p.selected ? [...p.selected] : []
      isChecked.value = !!p.result
    } else {
      selectedKeys.value = []
      isChecked.value = false
    }
    isRevealed.value = false
  }

  function buildQueue() {
    let filtered = [...questions.value]

    // Apply Source Filter if specified
    if (currentSource.value !== 'all') {
      filtered = filtered.filter((q) => {
        const src = (q.source || '').toLowerCase()
        const exam = (q.exam || '').toLowerCase()
        if (currentSource.value === 'exam') return exam.includes('fe') || src.includes('exam')
        if (currentSource.value === 'slides') return src.includes('slide') || exam.includes('slide')
        if (currentSource.value === 'quiz_pt') return src.includes('quiz') || exam.includes('pt')
        return true
      })
    }

    // Apply Mode Filter
    if (currentMode.value === 'seq') {
      queue.value = filtered
    } else if (currentMode.value === 'shuffle') {
      queue.value = [...filtered].sort(() => Math.random() - 0.5)
    } else if (currentMode.value === 'unanswered') {
      queue.value = filtered.filter((q) => {
        const p = progress.value[q.id] || progress.value[q.questionNumber]
        return !p?.result
      })
    } else if (currentMode.value === 'wrong') {
      queue.value = filtered.filter((q) => {
        const p = progress.value[q.id] || progress.value[q.questionNumber]
        return p?.result === 'bad'
      })
    } else if (currentMode.value === 'starred') {
      queue.value = filtered.filter((q) => {
        const p = progress.value[q.id] || progress.value[q.questionNumber]
        return !!p?.star
      })
    }

    pos.value = 0
    loadCurrentQuestionState()
  }

  // Catalog & Questions Loading
  async function fetchCatalog() {
    try {
      const response = await QuizAPI.getCatalog()
      subjects.value = response.data?.subjects || []
    } catch (err: any) {
      console.warn('Fallback getting quiz sets:', err)
      try {
        const res2 = await QuizAPI.getQuizSets()
        subjects.value = res2.data || []
      } catch (e2) {
        console.error('Failed to load catalog:', e2)
      }
    }
  }

  async function fetchQuestions(subjectId?: string) {
    const targetSub = subjectId || activeSubjectId.value
    activeSubjectId.value = targetSub
    loadingData.value = true
    isLocked.value = false
    error.value = null
    questions.value = []

    try {
      const response = await QuizAPI.getQuestions(targetSub, { pageSize: 2500 })
      const data: any = response.data
      const list: QuizQuestion[] = Array.isArray(data) ? data : (data?.questions || [])

      questions.value = list
      loadLocalProgress(targetSub)
      buildQueue()
    } catch (err: any) {
      if (err?.response?.status === 403) {
        isLocked.value = true
      } else {
        error.value = err?.response?.data?.message || err?.message || 'Lỗi khi tải danh sách câu hỏi.'
      }
    } finally {
      loadingData.value = false
    }
  }

  function selectSubject(subjectId: string) {
    if (activeSubjectId.value === subjectId && questions.value.length > 0) return
    activeSubjectId.value = subjectId
    fetchQuestions(subjectId)
  }

  // Interactivity
  function setMode(mode: QuizMode) {
    currentMode.value = mode
    buildQueue()
  }

  function setSourceFilter(source: SourceFilter) {
    currentSource.value = source
    buildQueue()
  }

  function selectOption(key: string) {
    const q = currentQuestion.value
    if (!q) return

    const chooseLimit = q.choose || 1

    if (chooseLimit <= 1) {
      // Single choice -> Immediate evaluation
      selectedKeys.value = [key]
      isChecked.value = true

      const answers = q.answers || []
      const isCorrect = answers.includes(key)

      const qKey = q.id
      const current = progress.value[qKey] || {}
      progress.value[qKey] = {
        ...current,
        selected: [key],
        result: isCorrect ? 'ok' : 'bad',
        answeredAt: new Date().toISOString()
      }
      saveLocalProgress()
    } else {
      // Multi choice -> Toggle option
      if (isChecked.value) return // already checked
      const idx = selectedKeys.value.indexOf(key)
      if (idx >= 0) {
        selectedKeys.value.splice(idx, 1)
      } else {
        if (selectedKeys.value.length < chooseLimit) {
          selectedKeys.value.push(key)
        }
      }
    }
  }

  function checkMultiChoice() {
    const q = currentQuestion.value
    if (!q || isChecked.value || selectedKeys.value.length === 0) return

    isChecked.value = true
    const correctAnswers = (q.answers || []).map((a) => a.trim().toUpperCase()).sort()
    const userAns = selectedKeys.value.map((a) => a.trim().toUpperCase()).sort()

    const isCorrect = correctAnswers.length === userAns.length &&
      correctAnswers.every((val, index) => val === userAns[index])

    const qKey = q.id
    const current = progress.value[qKey] || {}
    progress.value[qKey] = {
      ...current,
      selected: [...selectedKeys.value],
      result: isCorrect ? 'ok' : 'bad',
      answeredAt: new Date().toISOString()
    }
    saveLocalProgress()
  }

  function toggleReveal() {
    isRevealed.value = !isRevealed.value
  }

  function toggleStar(questionId?: string | number) {
    const qId = questionId !== undefined ? questionId : currentQuestion.value?.id
    if (!qId) return

    const current = progress.value[qId] || {}
    const newStar = !current.star
    progress.value[qId] = {
      ...current,
      star: newStar
    }
    saveLocalProgress()
  }

  function isStarred(questionId: string | number) {
    return !!(progress.value[questionId]?.star)
  }

  function next() {
    if (pos.value < queue.value.length - 1) {
      pos.value++
      loadCurrentQuestionState()
    }
  }

  function prev() {
    if (pos.value > 0) {
      pos.value--
      loadCurrentQuestionState()
    }
  }

  function jump(targetNum: number) {
    if (!targetNum || !queue.value.length) return
    const idx = queue.value.findIndex((q) => q.questionNumber === targetNum || q.id === String(targetNum) || q.id === targetNum)
    if (idx >= 0) {
      pos.value = idx
      loadCurrentQuestionState()
    } else {
      const clamped = Math.max(0, Math.min(queue.value.length - 1, targetNum - 1))
      pos.value = clamped
      loadCurrentQuestionState()
    }
  }

  function resetProgress() {
    progress.value = {}
    saveLocalProgress()
    buildQueue()
  }

  // Key Redemption
  async function redeemUnlockCode(code: string) {
    const res = await QuizAPI.unlockSubject(code)
    if (res.data?.success) {
      if (user.value) {
        user.value.unlockedSubjects = res.data.unlockedSubjects
        localStorage.setItem('fe_learn_user', JSON.stringify(user.value))
      }
      await fetchCatalog()
      await fetchQuestions(activeSubjectId.value)
    }
    return res.data
  }

  // Auth Sync
  function setUser(newUser: QuizUser | null, newToken?: string) {
    user.value = newUser
    if (newUser) {
      localStorage.setItem('fe_learn_user', JSON.stringify(newUser))
    } else {
      localStorage.removeItem('fe_learn_user')
    }

    if (newToken !== undefined) {
      token.value = newToken
      if (newToken) {
        localStorage.setItem('fe_learn_token', newToken)
      } else {
        localStorage.removeItem('fe_learn_token')
      }
    }
  }

  function logout() {
    setUser(null, '')
    isLocked.value = false
    fetchCatalog()
    fetchQuestions(activeSubjectId.value)
  }

  return {
    subjects,
    activeSubjectId,
    currentSubject,
    loadingData,
    isLocked,
    error,
    questions,
    queue,
    pos,
    currentMode,
    currentSource,
    selectedKeys,
    isChecked,
    isRevealed,
    progress,
    user,
    stats,
    modeLabel,
    currentQuestion,
    fetchCatalog,
    fetchQuestions,
    selectSubject,
    setMode,
    setSourceFilter,
    selectOption,
    checkMultiChoice,
    toggleReveal,
    toggleStar,
    isStarred,
    next,
    prev,
    jump,
    resetProgress,
    redeemUnlockCode,
    setUser,
    logout
  }
})
