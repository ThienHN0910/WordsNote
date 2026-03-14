<template>
  <div>
    <p
      class="ellipsis-text"
      :style="{
        display: '-webkit-box',
        WebkitBoxOrient: 'vertical',
        WebkitLineClamp: isExpanded ? 'unset' : lines,
        overflow: 'hidden',
      }"
      ref="textEl"
    >
      <slot></slot>
    </p>
    <button v-if="showToggle" @click="toggleExpand" class="toggle-btn">
      {{ isExpanded ? "Collapse" : "Expand" }}
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick } from "vue";

const props = defineProps({
  lines: { type: Number, default: 5 },
});

const isExpanded = ref(false);
const showToggle = ref(false);
const textEl = ref<HTMLElement | null>(null);

const toggleExpand = () => {
  isExpanded.value = !isExpanded.value;
};

onMounted(async () => {
  await nextTick();
  if (textEl.value) {
    const fullHeight = textEl.value.scrollHeight;
    const limitedHeight = textEl.value.clientHeight;
    if (fullHeight > limitedHeight) {
      showToggle.value = true;
    }
  }
});
</script>
