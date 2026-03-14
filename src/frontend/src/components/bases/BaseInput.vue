<template>
  <div :class="['', inputClass]">
    <div class="row">
      <label v-if="label" :for="id" class="col-4 app-base-input-label">{{ label }}</label>
      <input
      :id="id"
      :type="type"
      :value="modelValue"
      @input="onInput"
      :placeholder="placeholder"
      :disabled="disabled"
      class="app-base-input col-8"
      />
    </div>
    <div class="row">
      <div class="col-4"></div>
      <div class="col-8">
        <span class="app-base-input-error text-danger" v-if="errorMessage">{{ errorMessage }}!</span>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
const props = defineProps({
  modelValue: [String, Number],
  label: {
    type: String,
    default: "",
  },
  id: String,
  type: {
    type: String,
    default: "text",
  },
  placeholder: {
    type: String,
    default: "",
  },
  disabled: {
    type: Boolean,
    default: false,
  },
  inputClass: {
    type: String,
    default: "",
  },
  errorMessage: {
    type: String,
    default: "",
  },
});

const emit = defineEmits(["update:modelValue"]);

function onInput(event: Event) {
  const target = event.target as HTMLInputElement;
  emit("update:modelValue", target.value);
}
</script>
