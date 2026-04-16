<template>
  <div>
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;">
      <div>
        <p style="font-size: 15px; font-weight: 600; color: #1e293b;">
          {{ loading ? 'Loading...' : `${cards.length} local card${cards.length !== 1 ? 's' : ''} due` }}
        </p>
        <p style="font-size: 12px; color: #64748b; margin-top: 2px;">No login required</p>
      </div>
      <button
        @click="refresh"
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
        Refresh
      </button>
    </div>

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
        <div style="max-width: 200px; overflow: hidden;">
          <div style="font-size: 14px; color: #1e293b; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
            {{ card.front }}
          </div>
          <div style="font-size: 11px; color: #64748b; margin-top: 2px;">streak: {{ card.streak || 0 }}</div>
        </div>
        <div style="display: flex; gap: 6px;">
          <button
            style="padding: 4px 8px; border: 1px solid #fecaca; background: #fff1f2; color: #b91c1c; border-radius: 6px; font-size: 11px; cursor: pointer;"
            @click="review(card.id, 'hard')"
          >
            Hard
          </button>
          <button
            style="padding: 4px 8px; border: 1px solid #bbf7d0; background: #f0fdf4; color: #166534; border-radius: 6px; font-size: 11px; cursor: pointer;"
            @click="review(card.id, 'easy')"
          >
            Easy
          </button>
        </div>
      </div>

      <div style="margin-top: 16px; text-align: center;">
        <p style="font-size: 12px; color: #94a3b8; margin-bottom: 8px;">
          Highlights on webpages are saved to local cards automatically.
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
          Open Web App
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getDueLocalCards, reviewLocalCard } from '../../services/storage.js';

const cards = ref([]);
const loading = ref(true);
const errorMsg = ref('');

async function refresh() {
  loading.value = true;
  try {
    cards.value = await getDueLocalCards();
  } catch (err) {
    errorMsg.value = 'Failed to load local cards.';
  } finally {
    loading.value = false;
  }
}

async function review(cardId, difficulty) {
  await reviewLocalCard(cardId, difficulty);
  await refresh();
}

onMounted(refresh);

function openWebApp() {
  if (typeof chrome !== 'undefined' && chrome.tabs) {
    chrome.tabs.create({ url: 'http://localhost:5173' });
  } else {
    window.open('http://localhost:5173', '_blank');
  }
}
</script>

<style>
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}
</style>
