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
    radial-gradient(60% 50% at 10% 10%, rgba(255, 208, 143, 0.3), transparent 60%),
    radial-gradient(45% 35% at 92% 18%, rgba(165, 214, 255, 0.24), transparent 65%),
    #f7f3ec;
}

.app-main {
  padding-bottom: 3rem;
}
</style>
