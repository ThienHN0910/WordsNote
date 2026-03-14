<template>
  <div>
    <BaseInfinityScroll ref="infinityScroll" apiUrl="/api/Post" :itemComponent="Post" :limit="10"></BaseInfinityScroll>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import BaseInfinityScroll from '@/components/bases/BaseInfinityScroll.vue';
import Post from '@/components/CMS/Post.vue';

const infinityScroll = ref<any>(null);
</script>
