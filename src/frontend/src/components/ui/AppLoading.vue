<template>
  <div
    class="wn-loader"
    :class="[`wn-loader--${variant}`, `wn-loader--${size}`, { 'wn-loader--full': fullHeight }]"
    role="status"
    aria-live="polite"
  >
    <div class="wn-loader__orb" aria-hidden="true">
      <span class="wn-loader__ring wn-loader__ring--outer"></span>
      <span class="wn-loader__ring wn-loader__ring--inner"></span>
      <span class="wn-loader__core"></span>
    </div>

    <div class="wn-loader__copy">
      <p class="wn-loader__label">{{ label }}</p>
      <p v-if="description" class="wn-loader__description">{{ description }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
  variant?: 'inline' | 'page'
  size?: 'sm' | 'md' | 'lg'
  label?: string
  description?: string
  fullHeight?: boolean
}>(), {
  variant: 'inline',
  size: 'md',
  label: 'Loading...',
  description: '',
  fullHeight: false,
})

const {
  variant,
  size,
  label,
  description,
  fullHeight,
} = props
</script>

<style scoped>
.wn-loader {
  display: inline-flex;
  align-items: center;
  gap: 0.8rem;
  color: var(--wn-ink);
}

.wn-loader--page {
  width: 100%;
  justify-content: center;
  border: 1px solid color-mix(in srgb, var(--wn-primary) 20%, var(--wn-border));
  border-radius: 20px;
  padding: 1.1rem;
  background:
    radial-gradient(120% 120% at 0% 0%, color-mix(in srgb, var(--wn-primary) 14%, transparent), transparent 48%),
    var(--wn-surface);
  box-shadow: var(--wn-shadow-soft);
}

.wn-loader--full {
  min-height: 52vh;
}

.wn-loader__orb {
  position: relative;
  width: 38px;
  height: 38px;
  flex: 0 0 auto;
}

.wn-loader--sm .wn-loader__orb {
  width: 28px;
  height: 28px;
}

.wn-loader--lg .wn-loader__orb {
  width: 52px;
  height: 52px;
}

.wn-loader__ring {
  position: absolute;
  inset: 0;
  border-radius: 999px;
  border: 2px solid transparent;
  animation: wnSpin 1s linear infinite;
}

.wn-loader__ring--outer {
  border-top-color: var(--wn-primary);
  border-right-color: color-mix(in srgb, var(--wn-primary) 45%, transparent);
}

.wn-loader__ring--inner {
  inset: 6px;
  border-left-color: var(--wn-accent);
  border-bottom-color: color-mix(in srgb, var(--wn-accent) 45%, transparent);
  animation-direction: reverse;
  animation-duration: 0.8s;
}

.wn-loader__core {
  position: absolute;
  inset: 38%;
  border-radius: 999px;
  background: var(--wn-primary);
  box-shadow: 0 0 0 0 color-mix(in srgb, var(--wn-primary) 45%, transparent);
  animation: wnPulse 1.2s ease-in-out infinite;
}

.wn-loader__copy {
  display: grid;
  gap: 0.2rem;
}

.wn-loader__label {
  margin: 0;
  font-weight: 650;
  letter-spacing: 0.01em;
}

.wn-loader__description {
  margin: 0;
  color: var(--wn-muted);
  font-size: 0.92rem;
}

@keyframes wnSpin {
  to {
    transform: rotate(360deg);
  }
}

@keyframes wnPulse {
  0%,
  100% {
    transform: scale(0.92);
    box-shadow: 0 0 0 0 color-mix(in srgb, var(--wn-primary) 45%, transparent);
  }
  50% {
    transform: scale(1.1);
    box-shadow: 0 0 0 10px color-mix(in srgb, var(--wn-primary) 0%, transparent);
  }
}
</style>
