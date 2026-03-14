<template>
  <div class="app-leftbar">
    <h5 class="m-2">Home</h5>
    <ul>
      <RouterLink to="dashboard">
        <li>
          <h6>Dashboard</h6>
        </li>
      </RouterLink>
      <RouterLink to="about">
        <li>
          <h6>About</h6>
        </li>
      </RouterLink>
      <RouterLink to="examples">
        <li>
          <h6>Examples</h6>
        </li>
      </RouterLink>
    </ul>
  </div>
</template>

<script lang="ts" setup>
import { RouterLink } from "vue-router";
</script>