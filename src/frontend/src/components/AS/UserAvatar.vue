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
          <RouterLink to="/manage" class="dropdown-item">Manage collections</RouterLink>
        </li>
        <li>
          <button type="button" @click="logout" class="dropdown-item">Logout</button>
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

async function logout() {
  authStore.clearAuthToken()
  router.push({ name: 'login' })
}
</script>
