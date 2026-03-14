<template>
  <div :class="spinnerClass" role="status">
    <span class="visually-hidden">{{ hiddenText }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(defineProps<{
  type?: 'border' | 'grow';
  size?: 'sm' | 'lg' | '';
  color?: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info' | 'light' | 'dark' | '';
  hiddenText?: string;
}>(), {
  type: 'border',
  size: '',
  color: '',
  hiddenText: 'Loading...'
});

const spinnerClass = computed(() => {
  let base = `spinner-${props.type}`;
  if (props.size) base += ` spinner-${props.type}-${props.size}`;
  if (props.color) base += ` text-${props.color}`;
  return base;
});
</script>
