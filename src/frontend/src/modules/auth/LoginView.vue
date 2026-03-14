<template>
  <div class="min-h-screen bg-gradient-to-br from-indigo-50 to-blue-100 flex items-center justify-center px-4">
    <div class="bg-white rounded-2xl shadow-lg p-8 w-full max-w-md">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-indigo-600 mb-1">📝 WordsNote</h1>
        <p class="text-gray-500 text-sm">Sign in to your vocabulary dashboard</p>
      </div>

      <form @submit.prevent="handleGoogleLogin" class="space-y-5">
        <p class="text-sm text-gray-600 text-center">
          Sign in with your Google account via Supabase Auth.
        </p>

        <div v-if="authStore.error" class="bg-red-50 text-red-600 text-sm px-4 py-3 rounded-lg">
          {{ authStore.error }}
        </div>

        <button
          type="submit"
          :disabled="authStore.loading"
          class="w-full bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-300 text-white font-semibold py-2.5 rounded-lg transition-colors"
        >
          <span v-if="authStore.loading">Signing in...</span>
          <span v-else>Continue with Google</span>
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/authStore'

const router = useRouter()
const authStore = useAuthStore()

onMounted(async () => {
  await authStore.syncSession()

  if (!authStore.isAuthenticated) {
    return
  }

  const backendValid = await authStore.validateBackendToken()
  if (backendValid) {
    router.push('/')
  } else {
    authStore.error = 'Phiên đăng nhập hợp lệ ở Supabase nhưng backend từ chối token. Kiểm tra cấu hình SupabaseAuth (Authority/Audience/JwtSecret) ở backend.'
  }
})

async function handleGoogleLogin() {
  try {
    await authStore.loginWithGoogle()
  } catch {
    // error displayed via authStore.error
  }
}
</script>
