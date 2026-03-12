<template>
  <div class="min-h-screen bg-gray-50">
    <NavBar v-if="showNav" />
    <main :class="showNav ? 'pt-0' : ''">
      <RouterView />
    </main>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import NavBar from './components/NavBar.vue'

const route = useRoute()
const showNav = computed(() => route.name !== 'Login')
</script>
