<template>
  <header class="topbar-wrap">
    <div class="topbar">
      <RouterLink to="/" class="brand">
        <img src="/favicon.ico" alt="WordsNote" class="brand-icon" />
        <span>WordsNote</span>
      </RouterLink>

      <nav class="links">
        <RouterLink to="/" class="pill">Home</RouterLink>
        <RouterLink to="/learn" class="pill">Learn</RouterLink>
        <RouterLink to="/manage" class="pill">Manage</RouterLink>
      </nav>

      <ul class="auth-slot">
        <li>
          <button
            type="button"
            class="theme-toggle"
            :aria-label="`Switch to ${themeLabel} mode`"
            @click="toggleTheme"
          >
            <i :class="themeIcon"></i>
            <span>{{ themeLabel }}</span>
          </button>
        </li>
        <User />
      </ul>
    </div>
  </header>
</template>

<script lang="ts" setup>
import { computed } from 'vue'
import { storeToRefs } from 'pinia'
import { RouterLink } from 'vue-router'
import User from '@/components/AS/User.vue'
import { useThemeStore } from '@/stores/CFS/ThemeSettingStore'

const themeStore = useThemeStore()
const { isDark } = storeToRefs(themeStore)

const themeLabel = computed(() => (isDark.value ? 'Light' : 'Dark'))
const themeIcon = computed(() => (isDark.value ? 'fa-solid fa-sun' : 'fa-solid fa-moon'))

function toggleTheme() {
  isDark.value = !isDark.value
}
</script>

<style scoped>
.topbar-wrap {
  position: sticky;
  top: 0;
  z-index: 40;
  padding: 0.8rem 1rem 0;
}

.topbar {
  max-width: 1120px;
  margin: 0 auto;
  border: 1px solid var(--wn-topbar-border);
  border-radius: 16px;
  background: var(--wn-topbar-bg);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.55rem 0.8rem;
}

.brand {
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--wn-ink);
  font-weight: 700;
}

.brand-icon {
  width: 24px;
  height: 24px;
}

.links {
  display: inline-flex;
  flex-wrap: wrap;
  gap: 0.45rem;
}

.pill {
  text-decoration: none;
  border: 1px solid var(--wn-border);
  color: var(--wn-ink);
  padding: 0.32rem 0.75rem;
  border-radius: 999px;
  font-size: 0.93rem;
  background: var(--wn-surface);
}

.pill.router-link-active {
  background: var(--wn-primary);
  border-color: var(--wn-primary);
  color: var(--wn-on-primary);
}

.auth-slot {
  list-style: none;
  margin: 0;
  padding: 0;
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
}

.theme-toggle {
  border: 1px solid var(--wn-border);
  background: var(--wn-surface);
  color: var(--wn-ink);
  border-radius: 999px;
  padding: 0.34rem 0.72rem;
  display: inline-flex;
  align-items: center;
  gap: 0.42rem;
  font-size: 0.86rem;
}

.theme-toggle:hover {
  background: var(--wn-primary-soft);
}

@media (max-width: 760px) {
  .topbar {
    flex-wrap: wrap;
  }
}
</style>
