<template>
  <div class="infinite-list">
    <component v-for="item in items" :is="itemComponent" :key="item.id" :data="item" />
    <div v-if="!finished && items.length > 0" ref="loadMoreTrigger" class="load-trigger"></div>
    <div v-if="loading"><AppLoading /></div>
    <div v-if="finished">No More Data</div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref, type Component, type PropType } from 'vue'
import { useIntersectionObserver } from '@vueuse/core'
import apiClient from '@/apis/apiClient'
import AppLoading from '../ui/AppLoading.vue'
const props = defineProps({
  itemComponent: {
    type: Object as PropType<Component>,
    required: true,
  },
  apiUrl: { type: String, default: null },
  limit: { type: Number, default: 5 },
})

const items = ref<any[]>([])
const page = ref(1)
const loading = ref(false)
const finished = ref(false)

const loadMoreTrigger = ref<HTMLElement | null>(null)

const fetchData = async () => {
  if (loading.value || finished.value) return
  loading.value = true
  try {
    const res = await apiClient.get(props.apiUrl!, {
      params: { page: page.value, limit: props.limit },
    })
    const data = res.data

    if (!data || data.length === 0) {
      finished.value = true
    } else {
      items.value.push(...data)
      page.value++
    }
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

useIntersectionObserver(
  loadMoreTrigger,
  ([entry]) => {
    if (entry.isIntersecting && !loading.value && !finished.value) {
      fetchData()
    }
  },
  {
    root: null,
    rootMargin: '50px',
  }
)

onMounted(fetchData)
</script>
