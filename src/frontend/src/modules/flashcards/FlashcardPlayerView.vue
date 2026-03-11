<template>
  <div class="max-w-2xl mx-auto px-4 py-10">
    <!-- Header -->
    <div class="flex items-center gap-4 mb-8">
      <RouterLink :to="`/decks/${deckId}`" class="text-gray-400 hover:text-gray-600 transition-colors">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
      </RouterLink>
      <h1 class="text-xl font-bold text-gray-900">Study Session</h1>
    </div>

    <LoadingSpinner v-if="cardStore.loading" />

    <!-- All done state -->
    <div v-else-if="cardStore.dueCards.length === 0 && !cardStore.loading" class="text-center py-20">
      <div class="text-6xl mb-4">🎉</div>
      <h2 class="text-2xl font-bold text-gray-800 mb-2">All done!</h2>
      <p class="text-gray-500 mb-6">You've reviewed all due cards for this deck.</p>
      <RouterLink
        :to="`/decks/${deckId}`"
        class="bg-indigo-600 hover:bg-indigo-700 text-white font-medium px-6 py-2.5 rounded-lg transition-colors inline-block"
      >
        Back to Deck
      </RouterLink>
    </div>

    <template v-else>
      <!-- Progress bar -->
      <div class="mb-6">
        <div class="flex justify-between text-sm text-gray-500 mb-1">
          <span>Progress</span>
          <span>{{ reviewed }} / {{ total }} reviewed</span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-2">
          <div
            class="bg-indigo-600 h-2 rounded-full transition-all duration-300"
            :style="{ width: `${progressPercent}%` }"
          ></div>
        </div>
      </div>

      <!-- Flashcard -->
      <div class="card-container mb-8" @click="flipCard">
        <div class="card" :class="{ flipped: isFlipped }">
          <!-- Front -->
          <div class="card-face card-front bg-white border-2 border-gray-200 rounded-2xl shadow-md flex flex-col items-center justify-center p-10 cursor-pointer select-none">
            <p class="text-xs uppercase tracking-widest text-indigo-400 font-semibold mb-4">Front</p>
            <p class="text-3xl font-bold text-gray-900 text-center">{{ currentCard?.front }}</p>
            <p class="text-sm text-gray-400 mt-6">Click to reveal answer</p>
          </div>
          <!-- Back -->
          <div class="card-face card-back bg-indigo-50 border-2 border-indigo-200 rounded-2xl shadow-md flex flex-col items-center justify-center p-10 cursor-pointer select-none">
            <p class="text-xs uppercase tracking-widest text-indigo-400 font-semibold mb-4">Back</p>
            <p class="text-3xl font-bold text-indigo-800 text-center">{{ currentCard?.back }}</p>
            <p v-if="currentCard?.notes" class="text-sm text-gray-500 mt-4 italic">{{ currentCard.notes }}</p>
          </div>
        </div>
      </div>

      <!-- Review buttons (show after flip) -->
      <transition name="fade">
        <div v-if="isFlipped" class="grid grid-cols-4 gap-3">
          <button
            @click="handleReview(0)"
            class="flex flex-col items-center gap-1 bg-red-50 hover:bg-red-100 border border-red-200 text-red-700 font-semibold py-3 rounded-xl transition-colors"
          >
            <span class="text-lg">😓</span>
            <span class="text-sm">Again</span>
          </button>
          <button
            @click="handleReview(1)"
            class="flex flex-col items-center gap-1 bg-yellow-50 hover:bg-yellow-100 border border-yellow-200 text-yellow-700 font-semibold py-3 rounded-xl transition-colors"
          >
            <span class="text-lg">😐</span>
            <span class="text-sm">Hard</span>
          </button>
          <button
            @click="handleReview(2)"
            class="flex flex-col items-center gap-1 bg-green-50 hover:bg-green-100 border border-green-200 text-green-700 font-semibold py-3 rounded-xl transition-colors"
          >
            <span class="text-lg">😊</span>
            <span class="text-sm">Good</span>
          </button>
          <button
            @click="handleReview(3)"
            class="flex flex-col items-center gap-1 bg-blue-50 hover:bg-blue-100 border border-blue-200 text-blue-700 font-semibold py-3 rounded-xl transition-colors"
          >
            <span class="text-lg">😄</span>
            <span class="text-sm">Easy</span>
          </button>
        </div>
      </transition>

      <p v-if="!isFlipped" class="text-center text-sm text-gray-400 mt-2">Click the card to reveal the answer</p>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useCardStore } from '../../stores/cardStore'
import LoadingSpinner from '../../components/LoadingSpinner.vue'

const route = useRoute()
const cardStore = useCardStore()

const deckId = computed(() => route.params.id)
const isFlipped = ref(false)
const reviewed = ref(0)
const total = ref(0)

const currentCard = computed(() => cardStore.dueCards[0] ?? null)
const progressPercent = computed(() => (total.value > 0 ? (reviewed.value / total.value) * 100 : 0))

onMounted(async () => {
  await cardStore.fetchDueCards(deckId.value)
  total.value = cardStore.dueCards.length
})

function flipCard() {
  isFlipped.value = !isFlipped.value
}

async function handleReview(result) {
  if (!currentCard.value) return
  await cardStore.reviewCard(currentCard.value.id, result)
  reviewed.value++
  isFlipped.value = false
}
</script>

<style scoped>
.card-container {
  perspective: 1000px;
  height: 280px;
}

.card {
  position: relative;
  width: 100%;
  height: 100%;
  transform-style: preserve-3d;
  transition: transform 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

.card.flipped {
  transform: rotateY(180deg);
}

.card-face {
  position: absolute;
  width: 100%;
  height: 100%;
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
}

.card-back {
  transform: rotateY(180deg);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
