<template>
  <div class="extension-app">
    <div class="header">
      <span class="logo">📚</span>
      <h1 class="title">WordsNote</h1>
    </div>
    <div class="content">
      <LoginForm v-if="!isLoggedIn" @logged-in="handleLogin" />
      <DailyCards v-else @logged-out="handleLogout" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import LoginForm from './components/LoginForm.vue';
import DailyCards from './components/DailyCards.vue';
import { getToken } from '../services/storage.js';

const isLoggedIn = ref(false);

onMounted(async () => {
  const token = await getToken();
  isLoggedIn.value = !!token;
});

function handleLogin() {
  isLoggedIn.value = true;
}

function handleLogout() {
  isLoggedIn.value = false;
}
</script>

<style>
* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  width: 380px;
  font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  background: #f8fafc;
  color: #1e293b;
}

.extension-app {
  width: 380px;
  min-height: 300px;
  background: #ffffff;
}

.header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 16px 20px;
  background: #4f46e5;
  color: white;
}

.logo {
  font-size: 22px;
}

.title {
  font-size: 18px;
  font-weight: 600;
  letter-spacing: 0.3px;
}

.content {
  padding: 20px;
}
</style>
