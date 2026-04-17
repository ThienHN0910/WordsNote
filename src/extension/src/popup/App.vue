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
* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  width: 380px;
  font-family: 'Trebuchet MS', 'Segoe UI Variable', 'Segoe UI', sans-serif;
  background: linear-gradient(180deg, #eef4ff 0%, #f8fbff 100%);
  color: #0f172a;
}

.extension-app {
  width: 380px;
  min-height: 420px;
  background: #ffffff;
}

.header {
  padding: 14px 18px;
  background: linear-gradient(130deg, #0b4f8a 0%, #1d77c3 100%);
  color: #f8fbff;
}

.title {
  font-size: 20px;
  font-weight: 600;
  letter-spacing: 0.2px;
}

.eyebrow {
  font-size: 11px;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  opacity: 0.9;
  margin-bottom: 2px;
}

.content {
  padding: 14px;
}
</style>
