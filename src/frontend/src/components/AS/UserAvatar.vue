<template>
  <li class="nav-item user-avatar-item" ref="menuContainerRef">
    <button
      class="avatar-trigger-btn"
      type="button"
      :aria-expanded="isOpen"
      aria-label="User profile menu"
      @click="isOpen = !isOpen"
    >
      <img
        v-if="userPicture"
        :src="userPicture"
        class="avatar-img"
        :alt="userName"
        @error="onImageError"
      />
      <div v-else class="avatar-monogram">
        {{ userInitial }}
      </div>
      <span class="user-name-label">{{ userName }}</span>
      <i class="fa-solid fa-chevron-down caret-icon" :class="{ rotated: isOpen }"></i>
    </button>

    <div v-if="isOpen" class="avatar-dropdown-menu shadow-lg fade-in" @click="isOpen = false">
      <div class="user-header">
        <p class="user-display-name">{{ userName }}</p>
        <p class="user-display-email">{{ userEmail }}</p>
        <span v-if="isAdmin" class="admin-badge">Admin</span>
      </div>

      <div class="dropdown-divider"></div>

      <RouterLink to="/manage" class="menu-item">
        <i class="fa-solid fa-folder-tree me-2"></i>
        <span>Manage Collections</span>
      </RouterLink>

      <RouterLink to="/learn" class="menu-item">
        <i class="fa-solid fa-brain me-2"></i>
        <span>Learn Lab & SRS</span>
      </RouterLink>

      <RouterLink to="/quiz" class="menu-item">
        <i class="fa-solid fa-graduation-cap me-2"></i>
        <span>Quiz Workspace</span>
      </RouterLink>

      <div class="dropdown-divider"></div>

      <button type="button" @click="handleLogout" class="menu-item logout-item">
        <i class="fa-solid fa-arrow-right-from-bracket me-2"></i>
        <span>Logout</span>
      </button>
    </div>
  </li>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { useQuizStore } from '@/stores/WordsNote/QuizStore'

const authStore = useAuthStore()
const quizStore = useQuizStore()
const router = useRouter()

const isOpen = ref(false)
const menuContainerRef = ref<HTMLElement | null>(null)
const imageFailed = ref(false)

const currentUser = computed(() => authStore.currentUser || quizStore.user)

const userName = computed(() => {
  return currentUser.value?.name || currentUser.value?.email?.split('@')[0] || 'User'
})

const userEmail = computed(() => {
  return currentUser.value?.email || ''
})

const userPicture = computed(() => {
  if (imageFailed.value) return ''
  return currentUser.value?.picture || ''
})

const userInitial = computed(() => {
  const name = userName.value.trim()
  return name ? name.charAt(0).toUpperCase() : 'U'
})

const isAdmin = computed(() => {
  return Boolean(currentUser.value?.isAdmin)
})

function onImageError() {
  imageFailed.value = true
}

function handleLogout() {
  authStore.clearAuthToken()
  quizStore.logout()
  router.push({ name: 'login' })
}

function onClickOutside(event: MouseEvent) {
  if (menuContainerRef.value && !menuContainerRef.value.contains(event.target as Node)) {
    isOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', onClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', onClickOutside)
})
</script>

<style scoped>
.user-avatar-item {
  position: relative;
  list-style: none;
}

.avatar-trigger-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  padding: 0.25rem 0.6rem 0.25rem 0.35rem;
  border-radius: 999px;
  color: var(--wn-ink);
  font-size: 0.86rem;
  transition: all 0.18s ease;
}

.avatar-trigger-btn:hover {
  background: var(--wn-primary-soft);
  border-color: color-mix(in srgb, var(--wn-primary) 35%, transparent);
}

.avatar-img {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  object-fit: cover;
}

.avatar-monogram {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  background: var(--wn-primary);
  color: var(--wn-on-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.75rem;
}

.user-name-label {
  font-weight: 600;
  max-width: 110px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.caret-icon {
  font-size: 0.65rem;
  color: var(--wn-muted);
  transition: transform 0.2s ease;
}

.caret-icon.rotated {
  transform: rotate(180deg);
}

.avatar-dropdown-menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 230px;
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 16px;
  padding: 0.75rem 0.5rem;
  z-index: 1000;
  box-shadow: var(--wn-shadow-soft);
}

.user-header {
  padding: 0.35rem 0.65rem 0.5rem;
  text-align: left;
}

.user-display-name {
  margin: 0;
  font-weight: 700;
  font-size: 0.92rem;
  color: var(--wn-ink);
}

.user-display-email {
  margin: 0;
  font-size: 0.78rem;
  color: var(--wn-muted);
  word-break: break-all;
}

.admin-badge {
  display: inline-block;
  margin-top: 0.35rem;
  padding: 0.15rem 0.5rem;
  background: #f59e0b;
  color: #000;
  font-size: 0.68rem;
  font-weight: 800;
  border-radius: 6px;
  text-transform: uppercase;
}

.dropdown-divider {
  height: 1px;
  background: var(--wn-border);
  margin: 0.4rem 0;
}

.menu-item {
  display: flex;
  align-items: center;
  width: 100%;
  padding: 0.45rem 0.65rem;
  border-radius: 8px;
  text-decoration: none;
  font-size: 0.85rem;
  color: var(--wn-ink);
  background: transparent;
  border: none;
  text-align: left;
  transition: background 0.15s ease;
}

.menu-item:hover {
  background: var(--wn-primary-soft);
  color: var(--wn-primary);
}

.logout-item {
  color: #ef4444;
}

.logout-item:hover {
  background: rgba(239, 68, 68, 0.12);
  color: #dc2626;
}
</style>
