<template>
  <div class="px-4 py-4">
    <PostEditor :limit-image-upload="3" @post-created="handlePostCreated" />
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import PostEditor from '@/components/bases/BaseCKEditor.vue';

const router = useRouter();

const handlePostCreated = () => {
    router.push('/cms');
};
</script>
