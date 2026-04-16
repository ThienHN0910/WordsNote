<template>
  <div class="container mt-5">
    <div class="row d-flex justify-content-center">
      <div class="col-md-6">
        <div class="px-5 py-5" id="form1">
          <div class="forms-inputs mb-4">
            <BaseInput
              v-model="email"
              label="Email or User Name"
              :error-message="!isValidUserNameOrEmail(email) ? MSG_EMAIL_EN.REQUIRED : ''"
            />
          </div>
          <div class="forms-inputs mb-4">
            <BaseInput
              v-model="password"
              :class="{ 'is-invalid': !validPassword(password) }"
              label="Password"
              type="password"
              :errorMessage="!validPassword(password) ? MSG_PASSWORD_EN.MIN_LENGTH : ''"
            />
          </div>
          <div class="forms-inputs mb-4">
            <BaseButton
              @click="submit"
              :disabled="isLoading || !validPassword(password) || !isValidUserNameOrEmail(email)"
              class="justify-content-end"
            >
              {{ isLoading ? 'Loading...' : 'Login' }}
            </BaseButton>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref } from 'vue'
import { MSG_EMAIL_EN, MSG_PASSWORD_EN } from '@/constants/Auth.constant'
import { AuthService } from '@/services/AS/AuthService'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { useRoute, useRouter } from 'vue-router'
import BaseInput from '@/components/bases/BaseInput.vue'
import BaseButton from '@/components/bases/BaseButton.vue'

const email = ref('')
const password = ref('')
const submitted = ref(false)
const isLoading = ref(false)
const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const validPassword = (password: string) => {
  return password.length >= 8
}

const isValidUserNameOrEmail = (email: string) => {
  return email.length > 0
}

const resolveRedirectRoute = () => {
  const redirect = route.query.redirect
  return typeof redirect === 'string' && redirect.length > 0 ? redirect : { name: 'home' }
}

const submit = () => {
  isLoading.value = true
  if (validPassword(password.value) && isValidUserNameOrEmail(email.value)) {
    submitted.value = true
  }
  AuthService.login(email.value, password.value)
    .then((response) => {
      if (response.status === 200) {
        const token = response.data
        authStore.setAuthToken(token)
        router.push(resolveRedirectRoute())
      } else {
        alert('Login failed. Please check your credentials and try again.')
        submitted.value = false
      }
    })
    .catch((error) => {
      alert(`Login failed. Please check your credentials and try again. ${error}`)
      submitted.value = false
      isLoading.value = false
    })
    .finally(() => {
      isLoading.value = false
    })
}
</script>
