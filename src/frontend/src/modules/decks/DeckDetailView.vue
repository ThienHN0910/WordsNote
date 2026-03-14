<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div v-if="deckStore.loading && !deckStore.currentDeck" class="flex justify-center py-20">
      <LoadingSpinner />
    </div>

    <template v-else-if="deckStore.currentDeck">
      <!-- Header -->
      <div class="flex items-center gap-4 mb-6">
        <RouterLink to="/" class="text-gray-400 hover:text-gray-600 transition-colors">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </RouterLink>
        <div class="flex-1">
          <h1 class="text-2xl font-bold text-gray-900">{{ deckStore.currentDeck.name }}</h1>
          <p v-if="deckStore.currentDeck.description" class="text-gray-500 text-sm mt-0.5">
            {{ deckStore.currentDeck.description }}
          </p>
        </div>
        <div class="flex items-center gap-2">
          <RouterLink
            :to="`/decks/${deckStore.currentDeck.id}/play`"
            class="bg-indigo-600 hover:bg-indigo-700 text-white font-medium px-4 py-2 rounded-lg transition-colors text-sm"
          >
            Study
          </RouterLink>
          <button
            @click="handleReset"
            class="bg-yellow-50 hover:bg-yellow-100 text-yellow-700 font-medium px-4 py-2 rounded-lg transition-colors text-sm border border-yellow-200"
          >
            Reset Progress
          </button>
          <!-- Hidden file input for CSV import -->
          <input ref="csvInput" type="file" accept=".csv" class="hidden" @change="handleCsvImport" />
          <button
            @click="csvInput.click()"
            class="bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium px-4 py-2 rounded-lg transition-colors text-sm"
          >
            Import CSV
          </button>
        </div>
      </div>

      <!-- Add Card Form -->
      <div class="bg-white border border-gray-200 rounded-xl p-5 mb-6">
        <h2 class="font-semibold text-gray-800 mb-4">Add New Card</h2>
        <form @submit.prevent="handleAddCard" class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div>
            <label class="block text-xs font-medium text-gray-500 mb-1 uppercase tracking-wide">Front</label>
            <input
              v-model="newCard.front"
              type="text"
              required
              placeholder="Word or phrase"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-500 mb-1 uppercase tracking-wide">Back</label>
            <input
              v-model="newCard.back"
              type="text"
              required
              placeholder="Definition or translation"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-500 mb-1 uppercase tracking-wide">Notes</label>
            <div class="flex gap-2">
              <input
                v-model="newCard.notes"
                type="text"
                placeholder="Optional notes"
                class="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
              />
              <button
                type="submit"
                :disabled="cardStore.loading"
                class="bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-300 text-white font-medium px-4 py-2 rounded-lg text-sm transition-colors whitespace-nowrap"
              >
                Add
              </button>
            </div>
          </div>
        </form>
        <p v-if="cardStore.error" class="text-red-500 text-sm mt-2">{{ cardStore.error }}</p>
      </div>

      <!-- Cards List -->
      <div class="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div class="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
          <h2 class="font-semibold text-gray-800">Cards ({{ cardStore.cards.length }})</h2>
        </div>

        <LoadingSpinner v-if="cardStore.loading" />

        <div v-else-if="cardStore.cards.length === 0" class="text-center py-12 text-gray-400">
          No cards yet. Add your first card above.
        </div>

        <ul v-else class="divide-y divide-gray-100">
          <li
            v-for="card in cardStore.cards"
            :key="card.id"
            class="px-5 py-3 flex items-center gap-4 hover:bg-gray-50 transition-colors"
          >
            <div class="flex-1 grid grid-cols-2 gap-4">
              <div>
                <p class="text-xs text-gray-400 uppercase tracking-wide mb-0.5">Front</p>
                <p class="text-sm font-medium text-gray-800">{{ card.front }}</p>
              </div>
              <div>
                <p class="text-xs text-gray-400 uppercase tracking-wide mb-0.5">Back</p>
                <p class="text-sm text-gray-600">{{ card.back }}</p>
              </div>
            </div>
            <button
              @click="handleDeleteCard(card.id)"
              class="p-1.5 text-gray-300 hover:text-red-500 transition-colors rounded"
              title="Delete card"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </button>
          </li>
        </ul>
      </div>
    </template>

    <div v-else class="text-center py-20 text-gray-400">Deck not found.</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useDeckStore } from '../../stores/deckStore'
import { useCardStore } from '../../stores/cardStore'
import LoadingSpinner from '../../components/LoadingSpinner.vue'

const route = useRoute()
const deckStore = useDeckStore()
const cardStore = useCardStore()

const csvInput = ref(null)
const newCard = ref({ front: '', back: '', notes: '' })

onMounted(async () => {
  await deckStore.fetchDeck(route.params.id)
  await cardStore.fetchCards(route.params.id)
})

async function handleAddCard() {
  try {
    await cardStore.createCard({ ...newCard.value, deckId: route.params.id })
    newCard.value = { front: '', back: '', notes: '' }
  } catch {
    // error shown via cardStore.error
  }
}

async function handleDeleteCard(id) {
  if (confirm('Delete this card?')) {
    await cardStore.deleteCard(id)
  }
}

async function handleReset() {
  if (confirm('Reset all progress for this deck? This cannot be undone.')) {
    await deckStore.resetDeck(route.params.id)
  }
}

async function handleCsvImport(event) {
  const file = event.target.files?.[0]
  if (!file) return
  try {
    await deckStore.importCsv(route.params.id, file)
    await cardStore.fetchCards(route.params.id)
  } catch {
    // error shown via deckStore.error
  } finally {
    event.target.value = ''
  }
}
</script>
