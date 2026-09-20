<template>
  <div v-if="show" class="modal-backdrop" @click.self="$emit('close')">
    <div class="modal-card glass-panel fade-in">
      <div class="modal-header">
        <div class="header-title">
          <span class="icon">🔒</span>
          <h3>Mã mở khóa môn học</h3>
        </div>
        <button class="close-btn" @click="$emit('close')">&times;</button>
      </div>

      <div class="modal-body">
        <p class="modal-desc">
          Môn <strong>JFE301</strong> và <strong>JIT401</strong> cần mã mở khóa để truy cập bài học và đáp án. Mỗi mã chỉ sử dụng được <strong>1 lần duy nhất</strong> cho tài khoản của bạn.
        </p>

        <div class="input-group-custom">
          <input
            v-model="accessCode"
            type="text"
            placeholder="Nhập mã mở khóa (16 ký tự)..."
            class="code-input"
            :disabled="loading"
            @keydown.enter="onSubmit"
          />
          <button class="submit-btn" :disabled="loading || !accessCode.trim()" @click="onSubmit">
            <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
            <span>Mở khóa</span>
          </button>
        </div>

        <p v-if="errorMsg" class="error-msg fade-in">
          ⚠️ {{ errorMsg }}
        </p>

        <p v-if="successMsg" class="success-msg fade-in">
          🎉 {{ successMsg }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = withDefaults(
  defineProps<{
    show?: boolean
  }>(),
  {
    show: false
  }
)

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'redeem-key', payload: { code: string; onSuccess: (msg?: string) => void; onError: (msg?: string) => void }): void
}>()

const accessCode = ref('')
const loading = ref(false)
const errorMsg = ref('')
const successMsg = ref('')

watch(
  () => props.show,
  (newVal) => {
    if (newVal) {
      accessCode.value = ''
      errorMsg.value = ''
      successMsg.value = ''
    }
  }
)

function onSubmit() {
  if (!accessCode.value.trim() || loading.value) return
  loading.value = true
  errorMsg.value = ''
  successMsg.value = ''

  emit('redeem-key', {
    code: accessCode.value.trim(),
    onSuccess: (msg) => {
      loading.value = false
      successMsg.value = msg || 'Mở khóa môn học thành công!'
      setTimeout(() => {
        emit('close')
      }, 1200)
    },
    onError: (err) => {
      loading.value = false
      errorMsg.value = err || 'Mã mở khóa không hợp lệ hoặc đã được sử dụng.'
    }
  })
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.72);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1100;
  padding: 16px;
}

.modal-card {
  width: 100%;
  max-width: 480px;
  border-radius: 16px;
  overflow: hidden;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.2);
}

.modal-header {
  padding: 18px 24px;
  border-bottom: 1px solid var(--wn-card-border, #e2e8f0);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.header-title h3 {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 700;
  color: var(--wn-ink, #0f172a);
}

.icon {
  font-size: 1.2rem;
}

.close-btn {
  background: transparent;
  border: none;
  font-size: 1.5rem;
  color: var(--wn-ink-muted, #64748b);
  cursor: pointer;
}

.modal-body {
  padding: 24px;
}

.modal-desc {
  font-size: 0.9rem;
  line-height: 1.5;
  color: var(--wn-ink-muted, #64748b);
  margin-bottom: 18px;
}

.input-group-custom {
  display: flex;
  gap: 8px;
  margin-bottom: 14px;
}

.code-input {
  flex: 1;
  height: 44px;
  padding: 0 14px;
  border-radius: 8px;
  border: 1.5px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  font-family: monospace;
  font-size: 0.95rem;
  text-transform: uppercase;
  outline: none;
}

.code-input:focus {
  border-color: #2563eb;
}

.submit-btn {
  padding: 0 18px;
  background: #2563eb;
  border: none;
  border-radius: 8px;
  color: #fff;
  font-weight: 600;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.15s ease;
}

.submit-btn:hover:not(:disabled) {
  background: #1d4ed8;
}

.submit-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.error-msg {
  color: #dc2626;
  font-size: 0.84rem;
  margin: 8px 0 0;
}

.success-msg {
  color: #059669;
  font-size: 0.84rem;
  font-weight: 600;
  margin: 8px 0 0;
}
</style>
