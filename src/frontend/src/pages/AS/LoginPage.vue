<template>
  <div class="container mt-5">
    <div class="row d-flex justify-content-center">
      <div class="col-md-6 col-lg-5">
        <div class="px-5 py-5" id="form1">
          <h3 class="mb-3">Login with Google</h3>
          <p class="text-muted mb-4">
            Only <strong>{{ allowedEmail }}</strong> is allowed to sign in.
          </p>

          <div class="forms-inputs mb-3 d-flex justify-content-center">
            <div ref="googleButtonRef"></div>
          </div>

          <div v-if="isLoading" class="text-center text-muted mb-3">Signing in...</div>

          <div v-if="errorMessage" class="alert alert-danger mb-0" role="alert">
            {{ errorMessage }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { onMounted, ref } from 'vue'
import { AuthService } from '@/services/AS/AuthService'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { useRoute, useRouter } from 'vue-router'

const googleButtonRef = ref<HTMLElement | null>(null)
const isLoading = ref(false)
const errorMessage = ref('')
const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const allowedEmail = import.meta.env.VITE_GOOGLE_ALLOWED_EMAIL || 'hnt.vn.vn@gmail.com'
const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID

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
      throw new Error('Unable to sign in with Google.')
    }

    authStore.setAuthToken(response.data)
    router.push(resolveRedirectRoute())
  } catch (error) {
    errorMessage.value = `Login failed. ${error}`
  } finally {
    isLoading.value = false
  }
}

const loadGoogleScript = () => {
  return new Promise<void>((resolve, reject) => {
    if (window.google?.accounts?.id) {
      resolve()
      return
    }

    const existing = document.querySelector<HTMLScriptElement>('script[data-google-identity="true"]')
    if (existing) {
      existing.addEventListener('load', () => resolve(), { once: true })
      existing.addEventListener('error', () => reject(new Error('Failed to load Google script.')), { once: true })
      return
    }

    const script = document.createElement('script')
    script.src = 'https://accounts.google.com/gsi/client'
    script.async = true
    script.defer = true
    script.dataset.googleIdentity = 'true'
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('Failed to load Google script.'))
    document.head.appendChild(script)
  })
}

const initializeGoogleButton = async () => {
  if (!googleClientId) {
    errorMessage.value = 'Missing VITE_GOOGLE_CLIENT_ID in frontend .env.'
    return
  }

  if (!googleButtonRef.value) {
    errorMessage.value = 'Google button container is not available.'
    return
  }

  await loadGoogleScript()

  if (!window.google?.accounts?.id) {
    errorMessage.value = 'Google Identity API is unavailable.'
    return
  }

  window.google.accounts.id.initialize({
    client_id: googleClientId,
    callback: (response: { credential?: string }) => {
      if (!response.credential) {
        errorMessage.value = 'Google did not return a credential.'
        return
      }

      void onGoogleCredential(response.credential)
    },
  })

  googleButtonRef.value.innerHTML = ''
  window.google.accounts.id.renderButton(googleButtonRef.value, {
    theme: 'outline',
    size: 'large',
    shape: 'rectangular',
    text: 'signin_with',
    width: 320,
  })
}

onMounted(() => {
  void initializeGoogleButton()
})
</script>
