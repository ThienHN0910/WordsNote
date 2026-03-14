<template>
  <div ref="container"></div>
</template>

<script lang="ts" setup>
import { onMounted, onBeforeUnmount, ref, watch } from 'vue'
import React from 'react'
import ReactDOM from 'react-dom/client'

const props = defineProps<{ component: React.FC<any>; reactProps?: Record<string, any> }>()
const container = ref<HTMLElement | null>(null)
let root: ReactDOM.Root | null = null

const renderReact = () => {
  if (container.value && props.component) {
    if (!root) {
      root = ReactDOM.createRoot(container.value)
    }
    root.render(React.createElement(props.component, props.reactProps || {}))
  }
}

onMounted(renderReact)

watch(() => props.reactProps, renderReact, { deep: true })

onBeforeUnmount(() => {
  root?.unmount()
})
</script>
