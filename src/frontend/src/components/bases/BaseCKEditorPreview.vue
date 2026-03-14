<script setup lang="ts">
import { defineProps, computed } from "vue";
import DOMPurify from "dompurify";

const props = defineProps<{
  content: string;
}>();

const previewHTML = computed(() => {
  return DOMPurify.sanitize(props.content);
});
</script>

<template>
  <!-- Sử dụng class ck-content để áp dụng styling mặc định của CKEditor -->
  <div class="ck-content" v-html="previewHTML"></div>
</template>

<style scoped>
/* Thêm CSS cho nội dung HTML từ CKEditor nếu cần thiết */
</style>