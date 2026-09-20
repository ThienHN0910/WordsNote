<template>
  <div class="controls-container">
    <div class="toolbar-simple glass-panel">
      <!-- Inline Stats & Progress Bar -->
      <div class="stats-inline">
        <span><b>{{ stats.done }}</b> đã làm</span>
        <span class="ok"><b>{{ stats.ok }}</b> đúng</span>
        <span class="bad"><b>{{ stats.bad }}</b> sai</span>
        <span><b>{{ stats.left }}</b> còn</span>
        <div class="progress-wrap" :title="`Tiến độ: ${progressPercent}%`">
          <div class="progress-bar-fill" :style="{ width: `${progressPercent}%` }"></div>
        </div>
      </div>

      <!-- Toolbar Right: Jump to Question & Mode Filter Chips -->
      <div class="toolbar-right">
        <div class="jump">
          <span>Câu</span>
          <input
            v-model.number="targetQuestionNum"
            type="number"
            min="1"
            :max="totalQuestions"
            class="jump-input"
            @keydown.enter="handleJump"
          />
          <button type="button" class="btn-jump" @click="handleJump">Đi</button>
        </div>

        <div class="filters">
          <button
            type="button"
            class="chip"
            :class="{ 'active': currentMode === 'seq' }"
            title="Tất cả câu hỏi theo thứ tự"
            @click="$emit('set-mode', 'seq')"
          >
            Tất cả
          </button>
          <button
            type="button"
            class="chip"
            :class="{ 'active': currentMode === 'wrong' }"
            title="Các câu làm sai"
            @click="$emit('set-mode', 'wrong')"
          >
            Sai
          </button>
          <button
            type="button"
            class="chip"
            :class="{ 'active': currentMode === 'unanswered' }"
            title="Các câu chưa làm"
            @click="$emit('set-mode', 'unanswered')"
          >
            Chưa
          </button>
          <button
            type="button"
            class="chip star-chip"
            :class="{ 'active': currentMode === 'starred' }"
            title="Các câu đã đánh dấu sao"
            @click="$emit('set-mode', 'starred')"
          >
            ★
          </button>
          <button
            type="button"
            class="chip"
            :class="{ 'active': currentMode === 'shuffle' }"
            title="Thứ tự ngẫu nhiên"
            @click="$emit('set-mode', 'shuffle')"
          >
            <i class="fa-solid fa-shuffle me-1"></i> Ngẫu nhiên
          </button>
        </div>
      </div>
    </div>

    <!-- Source Filter Bar (if subject has multiple sources like PRM/JFE/JIT) -->
    <div v-if="hasSourceFilters" class="source-bar filters glass-panel">
      <span class="source-label"><i class="fa-solid fa-layer-group me-1"></i> Nguồn</span>
      <button
        type="button"
        class="chip"
        :class="{ 'active': currentSource === 'all' }"
        @click="$emit('set-source', 'all')"
      >
        Tất cả
      </button>
      <button
        type="button"
        class="chip"
        :class="{ 'active': currentSource === 'exam' }"
        @click="$emit('set-source', 'exam')"
      >
        Đề FE
      </button>
      <button
        type="button"
        class="chip"
        :class="{ 'active': currentSource === 'slides' }"
        @click="$emit('set-source', 'slides')"
      >
        Ôn thêm
      </button>
      <button
        type="button"
        class="chip chip-pt"
        :class="{ 'active': currentSource === 'quiz_pt' }"
        @click="$emit('set-source', 'quiz_pt')"
      >
        Quiz ôn
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { QuizMode, SourceFilter } from '@/stores/WordsNote/QuizStore'

const props = withDefaults(
  defineProps<{
    pos?: number
    queueLength?: number
    totalQuestions?: number
    currentMode?: QuizMode
    currentSource?: SourceFilter
    stats?: { done: number; ok: number; bad: number; left: number; total: number }
    hasSourceFilters?: boolean
  }>(),
  {
    pos: 0,
    queueLength: 0,
    totalQuestions: 0,
    currentMode: 'seq',
    currentSource: 'all',
    stats: () => ({ done: 0, ok: 0, bad: 0, left: 0, total: 0 }),
    hasSourceFilters: true
  }
)

const emit = defineEmits<{
  (e: 'jump', num: number): void
  (e: 'set-mode', mode: QuizMode): void
  (e: 'set-source', source: SourceFilter): void
}>()

const targetQuestionNum = ref<number>(1)

const progressPercent = computed(() => {
  if (!props.totalQuestions) return 0
  return Math.min(100, Math.round((props.stats.done / props.totalQuestions) * 100))
})

function handleJump() {
  if (targetQuestionNum.value) {
    emit('jump', targetQuestionNum.value)
  }
}
</script>

<style scoped>
.controls-container {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 12px;
}

.toolbar-simple {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 10px 14px;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 12px;
}

.stats-inline {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 10px 14px;
  font-size: 0.82rem;
  color: var(--wn-ink-muted, #64748b);
}

.stats-inline b {
  color: var(--wn-ink, #0f172a);
  font-weight: 700;
}

.stats-inline .ok b {
  color: #10b981;
}

.stats-inline .bad b {
  color: #ef4444;
}

.progress-wrap {
  width: 90px;
  height: 6px;
  background: rgba(148, 163, 184, 0.2);
  border-radius: 99px;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  width: 0%;
  background: linear-gradient(90deg, #2563eb, #10b981);
  transition: width 0.25s ease;
}

.toolbar-right {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 10px;
}

.jump {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.82rem;
  color: var(--wn-ink-muted, #64748b);
}

.jump-input {
  width: 58px;
  height: 28px;
  padding: 2px 6px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 6px;
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  font-size: 0.82rem;
  text-align: center;
  outline: none;
}

.jump-input:focus {
  border-color: #2563eb;
}

.btn-jump {
  height: 28px;
  padding: 0 10px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: rgba(148, 163, 184, 0.1);
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--wn-ink, #0f172a);
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-jump:hover {
  background: #2563eb;
  border-color: #2563eb;
  color: #fff;
}

.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.chip {
  padding: 4px 10px;
  font-size: 0.78rem;
  font-weight: 600;
  border-radius: 999px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: transparent;
  color: var(--wn-ink-muted, #64748b);
  cursor: pointer;
  transition: all 0.15s ease;
}

.chip:hover {
  border-color: #94a3b8;
  color: var(--wn-ink, #0f172a);
}

.chip.active {
  background: var(--wn-ink, #0f172a);
  border-color: var(--wn-ink, #0f172a);
  color: #fff;
}

.star-chip {
  color: #f59e0b;
}

.star-chip.active {
  background: #f59e0b;
  border-color: #f59e0b;
  color: #fff;
}

.source-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  border-radius: 8px;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
}

.source-label {
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--wn-ink-muted, #64748b);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.chip-pt {
  border-color: rgba(220, 38, 38, 0.4);
}

.chip-pt.active {
  background: #dc2626;
  border-color: #dc2626;
}
</style>
