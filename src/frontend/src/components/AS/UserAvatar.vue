<template>
  <li class="nav-item">
    <div class="dropdown">
      <button class="btn" type="button" data-bs-toggle="dropdown" aria-expanded="false">
        <img
          src="/avatar-default.png"
          class="rounded-circle avatar-img app-user-avatar"
          alt="User Avatar"
        />
      </button>
      <ul class="dropdown-menu dropdown-menu-end shadow-sm">
        <li>
          <RouterLink to="/profile" class="dropdown-item">Profile</RouterLink>
        </li>
        <li>
          <RouterLink @click="logout" to="/logout" class="dropdown-item">Logout</RouterLink>
        </li>
      </ul>
    </div>
  </li>
</template>
<script lang="ts" setup>
import { useAuthStore } from '@/stores/AS/AuthStore'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

function logout() {
  authStore.clearAuthToken()
  router.push({ name: 'login' })
}
</script>
