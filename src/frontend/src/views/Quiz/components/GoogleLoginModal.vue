<template>
  <div v-if="show" class="login-overlay" @click.self="$emit('close')">
    <div class="login-card glass-panel fade-in">
      <button class="modal-close-corner" title="Đóng" @click="$emit('close')">&times;</button>
      <div class="login-header">
        <div class="logo-badge">
          <span class="logo-icon">🎓</span>
        </div>
        <h1 class="login-title">FE Ôn Tập · WordsNote</h1>
        <p class="login-subtitle">Đăng nhập tài khoản Google để truy cập đầy đủ bài học, mở khóa JFE301 & JIT401</p>
      </div>

      <div class="login-body">
        <div id="googleBtnContainer" class="google-btn-wrapper"></div>
        <p v-if="errorMsg" class="error-banner">{{ errorMsg }}</p>

        <div class="features-list">
          <div class="feature-item">
            <span class="feature-icon">⚡</span>
            <span>Ngân hàng 2.254 câu hỏi MLN122, PRM393, JFE301, JIT401</span>
          </div>
          <div class="feature-item">
            <span class="feature-icon">🔑</span>
            <span>Kích hoạt mã mở khóa 1 lần cho JFE301 & JIT401</span>
          </div>
          <div class="feature-item">
            <span class="feature-icon">💾</span>
            <span>Lưu từ vựng trực tiếp vào bộ flashcard SRS WordsNote</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, watch } from 'vue'

const props = withDefaults(
  defineProps<{
    show?: boolean
    clientId?: string
    errorMsg?: string
  }>(),
  {
    show: true,
    clientId: () => import.meta.env.VITE_GOOGLE_CLIENT_ID || '819670782167-v0q9fis9hrpq2pj35sk9s9p9c42c441c.apps.googleusercontent.com',
    errorMsg: ''
  }
)

const emit = defineEmits<{
  (e: 'login-success', credential: string): void
  (e: 'login-error', msg: string): void
  (e: 'close'): void
}>()

import { loadGoogleIdentityScript } from '@/services/AS/GoogleIdentityLoader'

onMounted(() => {
  if (props.show) {
    void initGoogleAuth()
  }
})

watch(
  () => props.show,
  (newVal) => {
    if (newVal) {
      setTimeout(() => void initGoogleAuth(), 100)
    }
  }
)

async function initGoogleAuth() {
  try {
    await loadGoogleIdentityScript()
  } catch (err) {
    emit('login-error', 'Không thể tải Google Identity Service. Vui lòng thử lại.')
    return
  }

  const win = window as any
  if (win.google && win.google.accounts && win.google.accounts.id) {
    try {
      win.google.accounts.id.initialize({
        client_id: props.clientId,
        callback: handleCredentialResponse,
        auto_select: false
      })

      const btnContainer = document.getElementById('googleBtnContainer')
      if (btnContainer) {
        btnContainer.innerHTML = ''
        win.google.accounts.id.renderButton(btnContainer, {
          theme: 'outline',
          size: 'large',
          text: 'signin_with',
          shape: 'pill',
          width: 280
        })
      }
    } catch (e: any) {
      emit('login-error', e?.message || 'Lỗi khởi tạo nút Google Login')
    }
  }
}

function handleCredentialResponse(response: any) {
  if (response && response.credential) {
    emit('login-success', response.credential)
  } else {
    emit('login-error', 'Không nhận được Google Credential')
  }
}
</script>

<style scoped>
.login-overlay {
  position: fixed;
  inset: 0;
  background: rgba(11, 15, 25, 0.85);
  backdrop-filter: blur(12px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1200;
  padding: 24px;
}

.login-card {
  position: relative;
  width: 100%;
  max-width: 440px;
  padding: 36px 32px 32px;
  text-align: center;
  border-radius: 20px;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.3);
}

.modal-close-corner {
  position: absolute;
  top: 14px;
  right: 18px;
  background: transparent;
  border: none;
  font-size: 1.5rem;
  color: var(--wn-ink-muted, #64748b);
  cursor: pointer;
}

.modal-close-corner:hover {
  color: var(--wn-ink, #0f172a);
}

.logo-badge {
  width: 64px;
  height: 64px;
  background: linear-gradient(135deg, rgba(37, 99, 235, 0.15), rgba(139, 92, 246, 0.15));
  border: 1px solid rgba(37, 99, 235, 0.3);
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  font-size: 32px;
}

.login-title {
  font-size: 1.35rem;
  font-weight: 800;
  letter-spacing: -0.5px;
  margin-bottom: 6px;
  background: linear-gradient(135deg, #2563eb, #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.login-subtitle {
  font-size: 0.85rem;
  color: var(--wn-ink-muted, #64748b);
  line-height: 1.5;
  margin-bottom: 24px;
}

.google-btn-wrapper {
  display: flex;
  justify-content: center;
  margin-bottom: 20px;
  min-height: 44px;
}

.error-banner {
  color: #ef4444;
  font-size: 0.82rem;
  margin-bottom: 16px;
  background: rgba(239, 68, 68, 0.1);
  padding: 8px 12px;
  border-radius: 8px;
}

.features-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  text-align: left;
  background: rgba(148, 163, 184, 0.08);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 12px;
  padding: 14px;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 0.8rem;
  color: var(--wn-ink, #0f172a);
}

.feature-icon {
  font-size: 1rem;
}
</style>
