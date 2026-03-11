<template>
  <div>
    <p style="margin-bottom: 16px; color: #64748b; font-size: 14px;">
      Sign in to save and review your vocabulary cards.
    </p>
    <form @submit.prevent="handleSubmit">
      <div style="margin-bottom: 12px;">
        <label style="display: block; font-size: 13px; font-weight: 500; color: #374151; margin-bottom: 4px;">
          Username
        </label>
        <input
          v-model="username"
          type="text"
          placeholder="Enter username"
          required
          style="
            width: 100%;
            padding: 8px 12px;
            border: 1px solid #d1d5db;
            border-radius: 6px;
            font-size: 14px;
            outline: none;
            transition: border-color 0.15s;
          "
          @focus="e => e.target.style.borderColor = '#4f46e5'"
          @blur="e => e.target.style.borderColor = '#d1d5db'"
        />
      </div>
      <div style="margin-bottom: 16px;">
        <label style="display: block; font-size: 13px; font-weight: 500; color: #374151; margin-bottom: 4px;">
          Password
        </label>
        <input
          v-model="password"
          type="password"
          placeholder="Enter password"
          required
          style="
            width: 100%;
            padding: 8px 12px;
            border: 1px solid #d1d5db;
            border-radius: 6px;
            font-size: 14px;
            outline: none;
            transition: border-color 0.15s;
          "
          @focus="e => e.target.style.borderColor = '#4f46e5'"
          @blur="e => e.target.style.borderColor = '#d1d5db'"
        />
      </div>
      <div
        v-if="errorMsg"
        style="
          margin-bottom: 12px;
          padding: 8px 12px;
          background: #fef2f2;
          border: 1px solid #fecaca;
          border-radius: 6px;
          color: #dc2626;
          font-size: 13px;
        "
      >
        {{ errorMsg }}
      </div>
      <button
        type="submit"
        :disabled="loading"
        style="
          width: 100%;
          padding: 10px;
          background: #4f46e5;
          color: white;
          border: none;
          border-radius: 6px;
          font-size: 14px;
          font-weight: 500;
          cursor: pointer;
          transition: background 0.15s;
        "
        @mouseover="e => !loading && (e.target.style.background = '#4338ca')"
        @mouseout="e => e.target.style.background = loading ? '#9ca3af' : '#4f46e5'"
      >
        {{ loading ? 'Signing in...' : 'Sign In' }}
      </button>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { login } from '../../services/api.js';
import { saveToken } from '../../services/storage.js';

const emit = defineEmits(['logged-in']);

const username = ref('');
const password = ref('');
const loading = ref(false);
const errorMsg = ref('');

async function handleSubmit() {
  loading.value = true;
  errorMsg.value = '';
  try {
    const data = await login(username.value, password.value);
    saveToken(data.token);
    emit('logged-in');
  } catch (err) {
    errorMsg.value = 'Invalid username or password.';
  } finally {
    loading.value = false;
  }
}
</script>
