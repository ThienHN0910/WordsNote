<template>
  <div>
    <h1>About Us</h1>
    <p>{{ pageInfor }}</p>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { InforService } from '@/services/FFP/InforService';

const pageInfor = ref('no information');
onMounted(async () => {
  const response = await InforService.getInfor();
  console.log(response.data);
  
  pageInfor.value = response.data;
});
</script>
