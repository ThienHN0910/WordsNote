<template>
  <span class="wn-skeleton" :style="skeletonStyle" aria-hidden="true"></span>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  width?: string
  height?: string
  radius?: string
}>(), {
  width: '100%',
  height: '1rem',
  radius: '10px',
})

const skeletonStyle = computed(() => ({
  width: props.width,
  height: props.height,
  borderRadius: props.radius,
}))
</script>

<style scoped>
.wn-skeleton {
  display: block;
  position: relative;
  overflow: hidden;
  background: color-mix(in srgb, var(--wn-border) 65%, var(--wn-surface));
}

.wn-skeleton::after {
  content: '';
  position: absolute;
  inset: 0;
  transform: translateX(-100%);
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--wn-surface) 78%, transparent),
    transparent
  );
  animation: wnSkeletonWave 1.2s ease-in-out infinite;
}

@keyframes wnSkeletonWave {
  100% {
    transform: translateX(100%);
  }
}
</style>
