<template>
  <div>
    <p>Author: {{ props.data?.userId }}</p>
    <BaseCKEditorPreview :content="props.data?.content ?? ''"></BaseCKEditorPreview>
  </div>
</template>
<script setup lang="ts">
import type { Post } from '@/types/CMS/Post'
import BaseCKEditorPreview from '../bases/BaseCKEditorPreview.vue'

const props = defineProps<{ data?: Post }>()

</script>
