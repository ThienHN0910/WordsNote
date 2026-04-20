<template>
  <template v-if="isAuthenticated">
    <UserAvatar />
  </template>

  <template v-else>
    <li class="nav-item">
      <RouterLink to="/login" class="nav-link">Sign in</RouterLink>
    </li>
  </template>
</template>
<script lang="ts" setup>
import { storeToRefs } from 'pinia'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/AS/AuthStore'
import UserAvatar from '@/components/AS/UserAvatar.vue'

const authStore = useAuthStore()
const { isAuthenticated } = storeToRefs(authStore)
</script>
