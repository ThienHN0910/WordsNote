<template>
  <div>
   <div class="row">
      <div class="col-2">
         <Leftbar></Leftbar>
      </div>
      <div class="col-10">
         <RouterView></RouterView>
      </div>
   </div>
  </div>
</template>

<script setup lang="ts">
import Leftbar from "@/pages/Leftbar.vue";
import { RouterView } from "vue-router";
</script>
