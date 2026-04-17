<template>
  <div class="extension-app">
    <div class="header">
      <div>
        <p class="eyebrow">WordsNote Extension</p>
        <h1 class="title">Learn Lab</h1>
      </div>
    </div>
    <div class="content">
      <DailyCards />
    </div>
  </div>
</template>

<script setup>
import DailyCards from './components/DailyCards.vue';
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
</style>
