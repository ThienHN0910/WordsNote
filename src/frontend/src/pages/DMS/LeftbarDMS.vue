<template>
  <div class="app-leftbar">
    <h5 class="m-2">DMS</h5>
    <ul>
      <RouterLink to="dashboard">
        <li>
          <h6>Dashboard</h6>
        </li>
      </RouterLink>
      <RouterLink to="upload">
        <li>
          <h6>Upload</h6>
        </li>
      </RouterLink>
    </ul>
  </div>
</template>

<script lang="ts" setup>
import { RouterLink } from "vue-router";
</script>