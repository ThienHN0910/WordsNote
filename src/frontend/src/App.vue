<template>
  <div class="app-shell">
    <AppNavbar />
    <main class="app-main">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import AppNavbar from './components/AppNavbar.vue'
import { RouterView } from 'vue-router'
</script>

<style scoped>
.app-shell {
  min-height: 100vh;
  background:
    radial-gradient(60% 50% at 10% 10%, var(--wn-shell-glow-a), transparent 60%),
    radial-gradient(45% 35% at 92% 18%, var(--wn-shell-glow-b), transparent 65%),
    var(--wn-bg);
  transition: background 0.25s ease;
}

.app-main {
  padding-bottom: 3rem;
}
</style>
