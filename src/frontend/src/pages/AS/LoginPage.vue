<template>
  <div class="login-page-wrap">
    <div class="login-card glass-panel fade-in">
      <div class="login-brand">
        <div class="brand-badge">
          <i class="fa-solid fa-graduation-cap"></i>
        </div>
        <h1 class="login-title">WordsNote Sync</h1>
        <p class="login-subtitle">
          Đăng nhập tài khoản Google để đồng bộ bộ thẻ Flashcard SRS, lưu tiến độ ôn thi 2.254+ câu hỏi và kích hoạt môn học.
        </p>
      </div>

      <div class="login-action-box">
        <div class="google-slot" ref="googleButtonRef"></div>

        <div v-if="isLoading" class="loading-state">
          <AppLoading
            variant="inline"
            size="sm"
            label="Đang xác thực..."
            description="Đang kết nối phiên học tập WordsNote..."
          />
        </div>

        <div v-if="errorMessage" class="error-notice" role="alert">
          <i class="fa-solid fa-triangle-exclamation me-2"></i>
          <span>{{ errorMessage }}</span>
        </div>

        <div v-if="originWarning" class="origin-tip">
          <p class="origin-tip-title">
            <i class="fa-solid fa-circle-info me-1"></i> Lưu ý cấu hình Google OAuth Origin
          </p>
          <p class="origin-tip-body">
            Nếu nút đăng nhập Google không hiển thị, hãy kiểm tra danh sách <strong>Authorized JavaScript Origins</strong> trên Google Cloud Console bao gồm tên miền mới:
            <code class="d-block mt-1">https://words-note.thienhn.io.vn</code>
          </p>
        </div>
      </div>

      <div class="guest-bridge">
        <div class="divider">
          <span>HOẶC TRẢI NGHIỆM KHÔNG CẦN ĐĂNG NHẬP</span>
        </div>

        <div class="guest-actions">
          <RouterLink to="/quiz" class="guest-btn">
            <i class="fa-solid fa-book-open"></i>
            <span>Ngân hàng Quiz (2.254 câu)</span>
          </RouterLink>
          <RouterLink to="/learn" class="guest-btn">
            <i class="fa-solid fa-brain"></i>
            <span>Learn Lab & Flashcards</span>
          </RouterLink>
        </div>
      </div>

      <footer class="login-footer">
        <p>WordsNote tôn trọng quyền riêng tư. Dữ liệu học tập cá nhân được lưu trữ local-first an toàn.</p>
        <RouterLink to="/privacy-policy" class="privacy-link">Chính sách bảo mật</RouterLink>
      </footer>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { AuthService } from '@/services/AS/AuthService'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { useQuizStore } from '@/stores/WordsNote/QuizStore'
import { loadGoogleIdentityScript } from '@/services/AS/GoogleIdentityLoader'
import AppLoading from '@/components/ui/AppLoading.vue'

const googleButtonRef = ref<HTMLElement | null>(null)
const isLoading = ref(false)
const errorMessage = ref('')
const originWarning = ref(false)

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const quizStore = useQuizStore()

const adminEmail = (import.meta.env.VITE_GOOGLE_ALLOWED_EMAIL || 'hnt.vn.vn@gmail.com').toLowerCase()
const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID || '819670782167-v0q9fis9hrpq2pj35sk9s9p9c42c441c.apps.googleusercontent.com'

const resolveRedirectRoute = () => {
  const redirect = route.query.redirect
  return typeof redirect === 'string' && redirect.length > 0 ? redirect : { name: 'manageCollections' }
}

const onGoogleCredential = async (credential: string) => {
  errorMessage.value = ''
  isLoading.value = true
  try {
    const response = await AuthService.loginWithGoogle(credential)
    if (response.status !== 200 || !response.data) {
      throw new Error('Máy chủ không phản hồi phiên đăng nhập hợp lệ.')
    }

    const resData: any = response.data
    const token = resData?.token || resData?.Token || credential
    const backendUser = resData?.user || resData?.User

    const isAdmin = backendUser?.isAdmin || backendUser?.email?.toLowerCase() === adminEmail

    const userProfile = {
      email: backendUser?.email || '',
      name: backendUser?.name || backendUser?.email?.split('@')[0] || 'User',
      picture: backendUser?.picture,
      unlockedSubjects: isAdmin ? ['mln122', 'prm393', 'jfe301', 'jit401'] : (backendUser?.unlockedSubjects || []),
      isAdmin
    }

    // Synchronize both auth stores immediately!
    authStore.setAuthSession(token, userProfile)
    quizStore.setUser(userProfile, token)

    if (!authStore.hasAuthSession) {
      throw new Error('Phiên đăng nhập chưa nhận được JWT Token hợp lệ.')
    }

    router.push(resolveRedirectRoute())
  } catch (error: any) {
    console.warn('Backend login failed, fallback to JWT decode for offline study:', error)
    try {
      const base64Url = credential.split('.')[1]
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
      const jsonPayload = decodeURIComponent(
        window
          .atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      )
      const payload = JSON.parse(jsonPayload)
      const isAdmin = payload.email?.toLowerCase() === adminEmail

      const userProfile = {
        email: payload.email,
        name: payload.name || payload.email.split('@')[0],
        picture: payload.picture,
        unlockedSubjects: isAdmin ? ['mln122', 'prm393', 'jfe301', 'jit401'] : [],
        isAdmin
      }

      authStore.setAuthSession(credential, userProfile)
      quizStore.setUser(userProfile, credential)
      router.push(resolveRedirectRoute())
    } catch (e: any) {
      errorMessage.value = `Đăng nhập thất bại: ${error?.response?.data?.message || error?.message || error}`
    }
  } finally {
    isLoading.value = false
  }
}

const initializeGoogleButton = async () => {
  if (!googleClientId) {
    errorMessage.value = 'Thiếu cấu hình VITE_GOOGLE_CLIENT_ID.'
    return
  }

  if (!googleButtonRef.value) {
    return
  }

  try {
    await loadGoogleIdentityScript()
  } catch {
    originWarning.value = true
    errorMessage.value = 'Không thể tải thư viện Google Identity. Vui lòng kiểm tra kết nối mạng.'
    return
  }

  const win = window as any
  if (!win.google?.accounts?.id) {
    originWarning.value = true
    return
  }

  try {
    win.google.accounts.id.initialize({
      client_id: googleClientId,
      callback: (response: { credential?: string }) => {
        if (!response.credential) {
          errorMessage.value = 'Google không trả về thông tin đăng nhập.'
          return
        }

        void onGoogleCredential(response.credential)
      },
      auto_select: false
    })

    googleButtonRef.value.innerHTML = ''
    win.google.accounts.id.renderButton(googleButtonRef.value, {
      theme: 'outline',
      size: 'large',
      shape: 'pill',
      text: 'signin_with',
      width: 320,
    })
  } catch (err: any) {
    originWarning.value = true
    errorMessage.value = `Lỗi hiển thị nút Google: ${err?.message || err}`
  }
}

onMounted(() => {
  authStore.rehydrateFromPersistedState()
  if (authStore.hasAuthSession) {
    void router.replace(resolveRedirectRoute())
    return
  }

  void initializeGoogleButton()
})
</script>

<style scoped>
.login-page-wrap {
  min-height: calc(100vh - 120px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem 1rem;
}

.login-card {
  width: 100%;
  max-width: 480px;
  padding: 2.5rem 2.2rem;
  border-radius: 24px;
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  box-shadow: var(--wn-shadow-soft);
  text-align: center;
}

.brand-badge {
  width: 56px;
  height: 56px;
  margin: 0 auto 1.1rem;
  border-radius: 16px;
  background: var(--wn-primary-soft);
  color: var(--wn-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.6rem;
  border: 1px solid color-mix(in srgb, var(--wn-primary) 30%, transparent);
}

.login-title {
  font-size: 1.7rem;
  line-height: 1.2;
  margin: 0 0 0.5rem;
  color: var(--wn-ink);
}

.login-subtitle {
  font-size: 0.92rem;
  line-height: 1.5;
  color: var(--wn-muted);
  margin: 0 0 1.8rem;
}

.login-action-box {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.9rem;
  margin-bottom: 1.8rem;
}

.google-slot {
  min-height: 48px;
  display: flex;
  justify-content: center;
}

.loading-state {
  margin-top: 0.5rem;
}

.error-notice {
  font-size: 0.85rem;
  padding: 0.65rem 0.95rem;
  border-radius: 12px;
  background: rgba(239, 68, 68, 0.12);
  color: #dc2626;
  border: 1px solid rgba(239, 68, 68, 0.25);
  text-align: left;
  width: 100%;
}

.origin-tip {
  margin-top: 0.4rem;
  text-align: left;
  background: var(--wn-surface-soft);
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  padding: 0.75rem 0.9rem;
  font-size: 0.82rem;
  color: var(--wn-muted);
  width: 100%;
}

.origin-tip-title {
  margin: 0 0 0.25rem;
  font-weight: 600;
  color: var(--wn-ink);
}

.origin-tip-body {
  margin: 0;
  line-height: 1.4;
}

.origin-tip code {
  background: var(--wn-surface);
  padding: 0.15rem 0.35rem;
  border-radius: 6px;
  font-size: 0.78rem;
  color: var(--wn-primary);
  border: 1px solid var(--wn-border);
}

.divider {
  position: relative;
  text-align: center;
  margin: 1.6rem 0 1.2rem;
}

.divider::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 1px;
  background: var(--wn-border);
}

.divider span {
  position: relative;
  background: var(--wn-surface);
  padding: 0 0.65rem;
  font-size: 0.72rem;
  letter-spacing: 0.08em;
  color: var(--wn-muted);
}

.guest-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.65rem;
}

.guest-btn {
  text-decoration: none;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
  padding: 0.75rem 0.5rem;
  border-radius: 14px;
  background: var(--wn-surface-soft);
  border: 1px solid var(--wn-border);
  color: var(--wn-ink);
  font-size: 0.82rem;
  font-weight: 500;
  transition: all 0.18s ease;
}

.guest-btn i {
  font-size: 1.1rem;
  color: var(--wn-primary);
}

.guest-btn:hover {
  background: var(--wn-primary-soft);
  border-color: color-mix(in srgb, var(--wn-primary) 40%, transparent);
  transform: translateY(-1px);
}

.login-footer {
  margin-top: 1.8rem;
  padding-top: 1.2rem;
  border-top: 1px solid var(--wn-border);
  font-size: 0.78rem;
  color: var(--wn-muted);
}

.login-footer p {
  margin: 0 0 0.4rem;
  line-height: 1.4;
}

.privacy-link {
  color: var(--wn-primary);
  text-decoration: none;
  font-weight: 500;
}

.privacy-link:hover {
  text-decoration: underline;
}
</style>
