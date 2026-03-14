<template>
   <div class="row">
      <div class="col-2">
         <Leftbar></Leftbar>
      </div>
      <div class="col-10">
         <RouterView></RouterView>
      </div>
   </div>
</template>

<script lang="ts" setup>
import Leftbar from "@/pages/CMS/LeftbarCMS.vue";
</script>
