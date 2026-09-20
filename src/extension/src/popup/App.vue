<template>
  <div class="extension-app">
    <div class="header">
      <div>
        <p class="eyebrow">WordsNote Extension</p>
        <h1 class="title">{{ activeView === 'learn' ? 'Learn Lab' : 'Manage Lab' }}</h1>
      </div>
      <div class="view-switch">
        <button :class="{ active: activeView === 'learn' }" @click="activeView = 'learn'">Learn</button>
        <button :class="{ active: activeView === 'manage' }" @click="activeView = 'manage'">Manage</button>
      </div>
    </div>
    <div class="content">
      <DailyCards v-if="activeView === 'learn'" />
      <ManageCards v-else />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import DailyCards from './components/DailyCards.vue';
import ManageCards from './components/ManageCards.vue';

const activeView = ref('learn');
</script>

<style>
:root {
  --wn-bg: #f7f3ec;
  --wn-ink: #1f2333;
  --wn-muted: #5b6275;
  --wn-surface: #ffffff;
  --wn-surface-soft: #fff8ef;
  --wn-border: #e6dfd3;
  --wn-primary: #1f4ed8;
  --wn-on-primary: #ffffff;
  --wn-primary-soft: #edf2ff;
  --wn-accent: #f08d49;
  --wn-shell-glow-a: rgba(255, 208, 143, 0.3);
  --wn-shell-glow-b: rgba(165, 214, 255, 0.24);
  --wn-topbar-bg: rgba(255, 252, 246, 0.92);
  --wn-topbar-border: #e6e0d8;
  --wn-shadow-soft: 0 10px 28px rgba(31, 35, 51, 0.08);
}

@media (prefers-color-scheme: dark) {
  :root {
    --wn-bg: #101826;
    --wn-ink: #e6ebff;
    --wn-muted: #a4b0cc;
    --wn-surface: #172236;
    --wn-surface-soft: #1a2940;
    --wn-border: #2a3a58;
    --wn-primary: #7da5ff;
    --wn-on-primary: #0f1c34;
    --wn-primary-soft: #223454;
    --wn-accent: #ffb36b;
    --wn-shell-glow-a: rgba(62, 92, 145, 0.22);
    --wn-shell-glow-b: rgba(242, 144, 77, 0.16);
    --wn-topbar-bg: rgba(19, 31, 50, 0.92);
    --wn-topbar-border: #2a3c5f;
    --wn-shadow-soft: 0 12px 34px rgba(0, 0, 0, 0.35);
  }
}

* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  width: 380px;
  font-family: 'Space Grotesk', 'Segoe UI', Tahoma, sans-serif;
  background:
    radial-gradient(60% 50% at 10% 10%, var(--wn-shell-glow-a), transparent 60%),
    radial-gradient(45% 35% at 92% 18%, var(--wn-shell-glow-b), transparent 65%),
    var(--wn-bg);
  color: var(--wn-ink);
  padding: 10px;
}

.extension-app {
  width: 380px;
  min-height: 440px;
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 18px;
  box-shadow: var(--wn-shadow-soft);
  overflow: hidden;
}

.header {
  padding: 12px 16px;
  background: var(--wn-topbar-bg);
  border-bottom: 1px solid var(--wn-topbar-border);
  backdrop-filter: blur(8px);
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 10px;
}

.title {
  font-family: 'Fraunces', 'Georgia', serif;
  font-size: 20px;
  font-weight: 700;
  letter-spacing: 0.01em;
  color: var(--wn-ink);
}

.eyebrow {
  font-size: 11px;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: var(--wn-muted);
  margin-bottom: 2px;
}

.content {
  padding: 12px;
  background: linear-gradient(180deg, var(--wn-surface), var(--wn-surface-soft));
}

.view-switch {
  display: flex;
  gap: 6px;
}

.view-switch button {
  border: 1px solid var(--wn-border);
  background: var(--wn-surface);
  border-radius: 10px;
  padding: 6px 10px;
  font-size: 12px;
  color: var(--wn-ink);
  cursor: pointer;
}

.view-switch button.active {
  border-color: var(--wn-primary);
  background: var(--wn-primary-soft);
  color: var(--wn-primary);
  font-weight: 600;
}
</style>
