<template>
  <div>
    <!-- Header row -->
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;">
      <div>
        <p style="font-size: 15px; font-weight: 600; color: #1e293b;">
          {{ loading ? 'Loading...' : `${cards.length} card${cards.length !== 1 ? 's' : ''} due today` }}
        </p>
      </div>
      <button
        @click="handleLogout"
        style="
          padding: 5px 10px;
          font-size: 12px;
          color: #64748b;
          background: transparent;
          border: 1px solid #e2e8f0;
          border-radius: 5px;
          cursor: pointer;
        "
        @mouseover="e => e.target.style.background = '#f1f5f9'"
        @mouseout="e => e.target.style.background = 'transparent'"
      >
        Logout
      </button>
    </div>

    <!-- Error state -->
    <div
      v-if="errorMsg"
      style="
        padding: 10px 12px;
        background: #fef2f2;
        border: 1px solid #fecaca;
        border-radius: 6px;
        color: #dc2626;
        font-size: 13px;
        margin-bottom: 12px;
      "
    >
      {{ errorMsg }}
    </div>

    <!-- Loading skeleton -->
    <div v-if="loading">
      <div
        v-for="i in 3"
        :key="i"
        style="
          height: 44px;
          background: #f1f5f9;
          border-radius: 6px;
          margin-bottom: 8px;
          animation: pulse 1.5s infinite;
        "
      />
    </div>

    <!-- Empty state -->
    <div
      v-else-if="!errorMsg && cards.length === 0"
      style="
        text-align: center;
        padding: 32px 16px;
        color: #64748b;
      "
    >
      <div style="font-size: 36px; margin-bottom: 10px;">🎉</div>
      <p style="font-size: 14px; font-weight: 500;">All caught up!</p>
      <p style="font-size: 13px; margin-top: 4px;">No cards due today.</p>
    </div>

    <!-- Cards list -->
    <div v-else-if="!loading">
      <div
        v-for="card in cards"
        :key="card.id"
        style="
          display: flex;
          align-items: center;
          justify-content: space-between;
          padding: 10px 12px;
          background: #f8fafc;
          border: 1px solid #e2e8f0;
          border-radius: 6px;
          margin-bottom: 8px;
        "
      >
        <span style="font-size: 14px; color: #1e293b; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 240px;">
          {{ card.front }}
        </span>
        <span
          style="
            font-size: 11px;
            padding: 2px 8px;
            background: #ede9fe;
            color: #6d28d9;
            border-radius: 999px;
            font-weight: 500;
            flex-shrink: 0;
            margin-left: 8px;
          "
        >
          due
        </span>
      </div>

      <!-- Open web app link -->
      <div style="margin-top: 16px; text-align: center;">
        <p style="font-size: 12px; color: #94a3b8; margin-bottom: 8px;">
          Open the WordsNote web app to study
        </p>
        <button
          @click="openWebApp"
          style="
            padding: 8px 20px;
            background: #4f46e5;
            color: white;
            border: none;
            border-radius: 6px;
            font-size: 13px;
            font-weight: 500;
            cursor: pointer;
          "
          @mouseover="e => e.target.style.background = '#4338ca'"
          @mouseout="e => e.target.style.background = '#4f46e5'"
        >
          📖 Study Now
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { apiGet } from '../../services/api.js';
import { clearToken } from '../../services/storage.js';

const emit = defineEmits(['logged-out']);

const cards = ref([]);
const loading = ref(true);
const errorMsg = ref('');

onMounted(async () => {
  try {
    const data = await apiGet('/api/cards/due');
    cards.value = Array.isArray(data) ? data : (data.cards || []);
  } catch (err) {
    errorMsg.value = 'Failed to load cards. Please check your connection.';
  } finally {
    loading.value = false;
  }
});

function handleLogout() {
  clearToken();
  emit('logged-out');
}

function openWebApp() {
  if (typeof chrome !== 'undefined' && chrome.tabs) {
    chrome.tabs.create({ url: 'http://localhost:5000' });
  } else {
    window.open('http://localhost:5000', '_blank');
  }
}
</script>

<style>
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}
</style>
