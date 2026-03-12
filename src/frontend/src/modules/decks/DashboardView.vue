<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="flex items-center justify-between mb-8">
      <h1 class="text-2xl font-bold text-gray-900">My Decks</h1>
      <button
        @click="showNewDeckModal = true"
        class="bg-indigo-600 hover:bg-indigo-700 text-white font-medium px-4 py-2 rounded-lg transition-colors flex items-center gap-2"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
        </svg>
        New Deck
      </button>
    </div>

    <LoadingSpinner v-if="deckStore.loading" />

    <div v-else-if="deckStore.error" class="bg-red-50 text-red-600 px-4 py-3 rounded-lg">
      {{ deckStore.error }}
    </div>

    <div v-else-if="deckStore.decks.length === 0" class="text-center py-20">
      <div class="text-6xl mb-4">📚</div>
      <h2 class="text-xl font-semibold text-gray-600 mb-2">No decks yet</h2>
      <p class="text-gray-400 mb-6">Create your first deck to start learning vocabulary</p>
      <button
        @click="showNewDeckModal = true"
        class="bg-indigo-600 hover:bg-indigo-700 text-white font-medium px-6 py-2.5 rounded-lg transition-colors"
      >
        Create First Deck
      </button>
    </div>

    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <DeckCard
        v-for="deck in deckStore.decks"
        :key="deck.id"
        :deck="deck"
        @delete="handleDelete"
      />
    </div>

    <!-- New Deck Modal -->
    <div v-if="showNewDeckModal" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 px-4">
      <div class="bg-white rounded-2xl shadow-xl p-6 w-full max-w-md">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">Create New Deck</h2>
        <form @submit.prevent="handleCreateDeck" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Deck Name</label>
            <input
              v-model="newDeck.name"
              type="text"
              required
              placeholder="e.g. Spanish Vocabulary"
              class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea
              v-model="newDeck.description"
              rows="2"
              placeholder="Optional description"
              class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none"
            />
          </div>
          <div class="flex justify-end gap-3 pt-2">
            <button
              type="button"
              @click="showNewDeckModal = false"
              class="px-4 py-2 text-sm text-gray-600 hover:text-gray-800 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              :disabled="deckStore.loading"
              class="bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-300 text-white font-medium px-5 py-2 rounded-lg transition-colors text-sm"
            >
              Create
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useDeckStore } from '../../stores/deckStore'
import DeckCard from './DeckCard.vue'
import LoadingSpinner from '../../components/LoadingSpinner.vue'

const deckStore = useDeckStore()
const showNewDeckModal = ref(false)
const newDeck = ref({ name: '', description: '' })

onMounted(() => {
  deckStore.fetchDecks()
})

async function handleCreateDeck() {
  try {
    await deckStore.createDeck(newDeck.value)
    showNewDeckModal.value = false
    newDeck.value = { name: '', description: '' }
  } catch {
    // error displayed via deckStore.error
  }
}

async function handleDelete(id) {
  if (confirm('Delete this deck? This action cannot be undone.')) {
    await deckStore.deleteDeck(id)
  }
}
</script>
