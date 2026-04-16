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
        <User />
      </ul>
    </div>
  </header>
</template>

<script lang="ts" setup>
import { RouterLink } from 'vue-router'
import User from '@/components/AS/User.vue'
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
  border: 1px solid #e6e0d8;
  border-radius: 16px;
  background: rgba(255, 252, 246, 0.88);
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
  color: #21263a;
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
  border: 1px solid #d7deec;
  color: #2a3757;
  padding: 0.32rem 0.75rem;
  border-radius: 999px;
  font-size: 0.93rem;
  background: #fbfdff;
}

.pill.router-link-active {
  background: #214ed9;
  border-color: #214ed9;
  color: #fff;
}

.auth-slot {
  list-style: none;
  margin: 0;
  padding: 0;
}

@media (max-width: 760px) {
  .topbar {
    flex-wrap: wrap;
  }
}
</style>
