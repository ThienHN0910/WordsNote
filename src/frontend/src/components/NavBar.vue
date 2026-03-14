<template>
  <nav class="navbar navbar-expand-sm navbar">
    <div class="container-fluid">
      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#navbarSupportedContent"
        aria-controls="navbarSupportedContent"
        aria-expanded="false"
        aria-label="Toggle navigation"
      >
        <span class="navbar-toggler-icon"></span>
      </button>
      <div class="collapse navbar-collapse" id="navbarSupportedContent">
        <ul class="navbar-nav me-auto mb-2 mb-lg-0">
          <li class="nav-item">
            <RouterLink to="/" class="nav-link"><img src="/favicon.ico" alt="Home" class="app-logo" /></RouterLink>
          </li>
          <li class="nav-item">
            <RouterLink to="/dms/" class="nav-link">Document Manager System</RouterLink>
          </li>
          <li class="nav-item">
            <RouterLink to="/cms/" class="nav-link">Community System</RouterLink>
          </li>
          <li class="nav-item">
            <RouterLink to="/study" class="nav-link">WordsNote Study</RouterLink>
          </li>
        </ul>
        <ul class="navbar-nav ms-auto mb-2 mb-lg-0 align-items-center">
          <li class="nav-item">
            <div>
              <BaseButton @click="toggleTheme">
                <i :class="isDark ? 'fa-solid fa-moon' : 'fa-solid fa-sun'"></i>
              </BaseButton>
            </div>
          </li>
          <User></User>
        </ul>
      </div>
    </div>
  </nav>
</template>

<script lang="ts" setup>
import { RouterLink } from 'vue-router'
import BaseButton from '@/components/bases/BaseButton.vue'
import { useThemeStore } from '@/stores/CFS/ThemeSettingStore'
import User from '@/components/AS/User.vue'

const themeStore = useThemeStore()
const isDark = themeStore.isDark
function toggleTheme() {
  if (!document.startViewTransition) {
    themeStore.isDark = !themeStore.isDark
    return
  }
  document.startViewTransition(() => {
    themeStore.isDark = !themeStore.isDark
  })
}
</script>