<template>
  <div class="container mt-5">
    <div class="row d-flex justify-content-center">
      <div class="col-md-6">
        <div class="px-5 py-5" id="form1">
          <div class="forms-inputs mb-4">
            <BaseInput
              v-model="email"
              label="Email"
              :error-message="!email ? MSG_EMAIL_EN.REQUIRED : ''"
            />
          </div>
          <div class="forms-inputs mb-4">
            <BaseInput
              v-model="username"
              label="Username"
              :error-message="!username ? MSG_USERNAME_EN.REQUIRED : ''"
            />
          </div>
          <div class="forms-inputs mb-4">
            <BaseInput
              v-model="password"
              label="Password"
              type="password"
              :error-message="!validPassword(password) ? MSG_PASSWORD_EN.MIN_LENGTH : ''"
            />
          </div>
          <div class="forms-inputs mb-4">
            <BaseInput
              v-model="confirmPassword"
              label="Confirm Password"
              type="password"
              :error-message="
                !passwordsMatch(password, confirmPassword) ? MSG_PASSWORD_EN.CONFIRM_MATCH : ''
              "
            />
          </div>
          <div class="mb-3">
            <BaseButton
              :disabled="
                isLoading || !validPassword(password) || !passwordsMatch(password, confirmPassword)
              "
              @click="submit"
            >
              {{ isLoading ? 'Loading...' : 'Register' }}
            </BaseButton>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref } from 'vue'
import { MSG_EMAIL_EN, MSG_PASSWORD_EN, MSG_USERNAME_EN } from '@/constants/Auth.constant'
import { AuthService } from '@/services/AS/AuthService'
import { useRouter } from 'vue-router'
import BaseInput from '@/components/bases/BaseInput.vue'
import BaseButton from '@/components/bases/BaseButton.vue'

const email = ref('')
const password = ref('')
const submitted = ref(false)
const isLoading = ref(false)
const router = useRouter()
const confirmPassword = ref('')
const username = ref('')

const validPassword = (password: string) => {
  return password.length >= 8
}
const passwordsMatch = (password: string, confirmPassword: string) => {
  return password === confirmPassword
}
const submit = () => {
  isLoading.value = true
  if (validPassword(password.value)) {
    submitted.value = true
  }
  AuthService.register(email.value, username.value, password.value)
    .then((response) => {
      if (response.status === 200) {
        const token = response.data.token
        localStorage.setItem('auth_token', token)
        router.push({ name: 'home' })
      } else {
        alert('Register failed. Please check your credentials and try again.')
        submitted.value = false
      }
    })
    .catch((error) => {
      alert(`Register failed. Please check your credentials and try again. ${error}`)
      submitted.value = false
      isLoading.value = false
    })
    .finally(() => {
      isLoading.value = false
    })
}
</script>
