<template>
  <div class="bg-white rounded-xl border border-gray-200 shadow-sm hover:shadow-md transition-shadow p-5 flex flex-col gap-4">
    <div class="flex-1">
      <div class="flex items-start justify-between gap-2">
        <h3 class="font-semibold text-gray-900 text-lg leading-tight">{{ deck.name }}</h3>
        <span
          v-if="dueCount > 0"
          class="bg-red-100 text-red-600 text-xs font-semibold px-2 py-0.5 rounded-full whitespace-nowrap"
        >
          {{ dueCount }} due
        </span>
      </div>
      <p v-if="deck.description" class="text-sm text-gray-500 mt-1 line-clamp-2">{{ deck.description }}</p>
      <p class="text-sm text-gray-400 mt-2">{{ cardCount }} card{{ cardCount !== 1 ? 's' : '' }}</p>
    </div>

    <div class="flex items-center gap-2 pt-1 border-t border-gray-100">
      <RouterLink
        :to="`/decks/${deckId}/play`"
        :class="{ 'pointer-events-none opacity-50': !deckId }"
        class="flex-1 text-center bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium py-2 rounded-lg transition-colors"
      >
        Study
      </RouterLink>
      <RouterLink
        :to="`/decks/${deckId}`"
        :class="{ 'pointer-events-none opacity-50': !deckId }"
        class="flex-1 text-center bg-gray-100 hover:bg-gray-200 text-gray-700 text-sm font-medium py-2 rounded-lg transition-colors"
      >
        Manage
      </RouterLink>
      <button
        @click="$emit('delete', deckId)"
        :disabled="!deckId"
        class="p-2 text-gray-400 hover:text-red-500 transition-colors rounded-lg hover:bg-red-50"
        title="Delete deck"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  deck: {
    type: Object,
    required: true,
  },
})

defineEmits(['delete'])

const deckId = computed(() => props.deck.id ?? props.deck.Id ?? props.deck.deck_id ?? null)

const cardCount = computed(() => props.deck.card_count ?? props.deck.cardCount ?? 0)
const dueCount = computed(() => props.deck.due_count ?? props.deck.dueCount ?? 0)
</script>
