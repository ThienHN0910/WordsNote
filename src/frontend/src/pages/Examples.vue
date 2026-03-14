<template>
  <main>
    <br />

    <BaseDropUpload
      v-model="selectedFiles"
      :extensionAccept="'mp3, mp4, png, jpg'"
      :maxFiles="5"
      @files-selected="onFilesSelected"
    />
  </main>
</template>

<script setup lang="ts">
import BaseDropUpload from '@/components/bases/BaseDropUpload.vue'
import { ref } from 'vue'

const selectedFiles = ref<File[]>([])

function onFilesSelected(files: File[]) {
  selectedFiles.value = files
}
</script>
