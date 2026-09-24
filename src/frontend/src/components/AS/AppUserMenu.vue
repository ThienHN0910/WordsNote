<template>
  <template v-if="hasSession">
    <UserAvatar />
  </template>

  <template v-else>
    <li class="nav-item">
      <RouterLink to="/login" class="nav-sign-in-btn">
        <i class="fa-solid fa-arrow-right-to-bracket"></i>
        <span>Sign in</span>
      </RouterLink>
    </li>
  </template>
</template>

<script lang="ts" setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { useQuizStore } from '@/stores/WordsNote/QuizStore'
import UserAvatar from '@/components/AS/UserAvatar.vue'

const authStore = useAuthStore()
const quizStore = useQuizStore()

const hasSession = computed(() => {
  return authStore.isAuthenticated || Boolean(quizStore.user)
})
</script>

<style scoped>
.nav-sign-in-btn {
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  background: var(--wn-primary);
  color: var(--wn-on-primary);
  padding: 0.32rem 0.8rem;
  border-radius: 999px;
  font-size: 0.86rem;
  font-weight: 600;
  transition: all 0.18s ease;
}

.nav-sign-in-btn:hover {
  filter: brightness(1.1);
  transform: translateY(-1px);
}
</style>
