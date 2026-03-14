<template>
  <div class="app-leftbar">
    <h5 class="m-2">CMS</h5>
    <ul>
      <RouterLink to="create-post">
        <li>
          <h6>Create Post</h6>
        </li>
      </RouterLink>
      <RouterLink to="dashboard">
        <li>
          <h6>Dashboard</h6>
        </li>
      </RouterLink>
    </ul>
  </div>
</template>

<script lang="ts" setup>
import { RouterLink } from "vue-router";
</script>