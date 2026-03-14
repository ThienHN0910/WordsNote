<template>
  <div :class="inputClass">
    <button
      class="btn btn-primary app-base-button"
      :type="type"
      :disabled="disabled"
      @click="onClick"
    >
      <slot>{{ buttonText }}</slot>
    </button>
  </div>
</template>

<script setup lang="ts">
import { defineProps, defineEmits } from "vue";

const props = defineProps({
  buttonText: {
    type: String,
    default: "button",
  },
  inputClass: {
    type: String,
    default: "",
  },
  type: {
    type: String as () => "button" | "submit" | "reset",
    default: "button",
  },
  disabled: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits<{
  (e: "click", event: MouseEvent): void;
}>();

function onClick(event: MouseEvent) {
  emit("click", event);
}
</script>
