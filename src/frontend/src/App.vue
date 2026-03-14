<template>
  <div>
    <Navbar></Navbar>      
  </div>
  <div class="container-fluid">
      <RouterView></RouterView>
  </div>
</template>

<script setup lang="ts">
import Navbar from './components/Navbar.vue'
import { RouterView } from 'vue-router' 

</script>

<style scoped></style>
