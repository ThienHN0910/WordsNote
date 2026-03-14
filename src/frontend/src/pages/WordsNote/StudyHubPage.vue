<template>
  <div class="study-page container py-4">
    <header class="study-header mb-4">
      <h1 class="display-6 fw-bold mb-2">WordsNote Study Hub</h1>
      <p class="text-muted mb-0">Quan ly collection the hoc va vao che do luyen tap theo kieu Quizlet.</p>
    </header>

    <div class="row g-4">
      <div class="col-12 col-lg-4">
        <section class="card shadow-sm border-0 h-100">
          <div class="card-body">
            <h2 class="h5 mb-3">Tao Collection</h2>
            <form class="vstack gap-2" @submit.prevent="handleCreateDeck">
              <input
                v-model="newDeck.title"
                class="form-control"
                type="text"
                placeholder="Ten collection"
                required
              />
              <textarea
                v-model="newDeck.description"
                class="form-control"
                placeholder="Mo ta ngan"
                rows="3"
              />
              <button class="btn btn-primary" type="submit">Tao collection</button>
            </form>

            <hr class="my-4" />

            <h2 class="h5 mb-3">Danh sach Collection</h2>
            <div v-if="!deckList.length" class="text-muted small">Chua co collection nao.</div>
            <div v-else class="vstack gap-2">
              <button
                v-for="deck in deckList"
                :key="deck.id"
                class="btn btn-light text-start p-3 border"
                :class="{ 'active-deck': selectedDeckId === deck.id }"
                @click="selectedDeckId = deck.id"
              >
                <div class="fw-semibold">{{ deck.title }}</div>
                <div class="small text-muted">{{ getDeckStats(deck.id).totalCards }} cards</div>
              </button>
            </div>
          </div>
        </section>
      </div>

      <div class="col-12 col-lg-8">
        <section v-if="selectedDeck" class="card shadow-sm border-0 h-100">
          <div class="card-body">
            <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
              <div>
                <h2 class="h4 mb-1">{{ selectedDeck.title }}</h2>
                <p class="text-muted mb-0">{{ selectedDeck.description || 'Khong co mo ta' }}</p>
              </div>
              <div class="d-flex gap-2">
                <button class="btn btn-outline-danger btn-sm" @click="handleDeleteDeck">Xoa</button>
                <button class="btn btn-success btn-sm" @click="goToSession">Bat dau hoc</button>
              </div>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-6 col-md-4">
                <div class="metric-box">
                  <div class="metric-number">{{ deckStats.totalCards }}</div>
                  <div class="metric-label">Tong cards</div>
                </div>
              </div>
              <div class="col-6 col-md-4">
                <div class="metric-box">
                  <div class="metric-number">{{ deckStats.dueCards }}</div>
                  <div class="metric-label">Can on</div>
                </div>
              </div>
              <div class="col-12 col-md-4">
                <div class="metric-box">
                  <div class="metric-number">{{ deckStats.masteredCards }}</div>
                  <div class="metric-label">Da nho vung</div>
                </div>
              </div>
            </div>

            <h3 class="h5 mb-2">Them / Sua Card</h3>
            <form class="row g-2 mb-4" @submit.prevent="handleSubmitCard">
              <div class="col-12 col-md-6">
                <input
                  v-model="cardForm.front"
                  class="form-control"
                  type="text"
                  placeholder="Mat truoc"
                  required
                />
              </div>
              <div class="col-12 col-md-6">
                <input
                  v-model="cardForm.back"
                  class="form-control"
                  type="text"
                  placeholder="Mat sau"
                  required
                />
              </div>
              <div class="col-12 col-md-6">
                <input
                  v-model="cardForm.hint"
                  class="form-control"
                  type="text"
                  placeholder="Goi y (tu chon)"
                />
              </div>
              <div class="col-12 col-md-6">
                <input
                  v-model="cardForm.tagsText"
                  class="form-control"
                  type="text"
                  placeholder="Tags, phan tach bang dau phay"
                />
              </div>
              <div class="col-12 d-flex gap-2">
                <button class="btn btn-primary" type="submit">
                  {{ cardForm.editingCardId ? 'Luu card' : 'Them card' }}
                </button>
                <button v-if="cardForm.editingCardId" class="btn btn-outline-secondary" type="button" @click="resetCardForm">
                  Huy sua
                </button>
              </div>
            </form>

            <h3 class="h5 mb-2">Cards</h3>
            <div v-if="!deckCards.length" class="text-muted">Collection nay chua co card.</div>
            <div v-else class="vstack gap-2">
              <div v-for="card in deckCards" :key="card.id" class="card-item p-3">
                <div class="d-flex justify-content-between gap-2">
                  <div>
                    <div class="fw-semibold">{{ card.front }}</div>
                    <div class="text-muted">{{ card.back }}</div>
                    <div v-if="card.hint" class="small fst-italic mt-1">Hint: {{ card.hint }}</div>
                    <div class="small mt-2">
                      <span class="badge text-bg-light me-1" v-for="tag in card.tags" :key="tag">{{ tag }}</span>
                    </div>
                  </div>
                  <div class="d-flex flex-column gap-2">
                    <button class="btn btn-outline-primary btn-sm" @click="startEditCard(card.id)">Sua</button>
                    <button class="btn btn-outline-danger btn-sm" @click="removeCard(card.id)">Xoa</button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <section v-else class="card shadow-sm border-0 h-100">
          <div class="card-body text-center text-muted d-flex align-items-center justify-content-center">
            Chon mot collection de quan ly cards.
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useStudyStore } from '@/stores/WordsNote/StudyStore'

const router = useRouter()
const studyStore = useStudyStore()

const selectedDeckId = ref('')

const newDeck = reactive({
  title: '',
  description: '',
})

const cardForm = reactive({
  editingCardId: '',
  front: '',
  back: '',
  hint: '',
  tagsText: '',
})

const deckList = computed(() => studyStore.deckList)
const selectedDeck = computed(() => studyStore.getDeckById(selectedDeckId.value))
const deckCards = computed(() => {
  if (!selectedDeckId.value) return []
  return studyStore.getDeckCards(selectedDeckId.value)
})
const deckStats = computed(() => {
  if (!selectedDeckId.value) {
    return { totalCards: 0, dueCards: 0, masteredCards: 0 }
  }
  return studyStore.getDeckStats(selectedDeckId.value)
})

function getDeckStats(deckId: string) {
  return studyStore.getDeckStats(deckId)
}

function handleCreateDeck() {
  const title = newDeck.title.trim()
  if (!title) return

  const deck = studyStore.createDeck(title, newDeck.description)
  selectedDeckId.value = deck.id
  newDeck.title = ''
  newDeck.description = ''
}

function parseTags(tagsText: string) {
  return tagsText
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean)
}

function handleSubmitCard() {
  if (!selectedDeckId.value) return

  const front = cardForm.front.trim()
  const back = cardForm.back.trim()
  if (!front || !back) return

  const tags = parseTags(cardForm.tagsText)

  if (cardForm.editingCardId) {
    studyStore.updateCard(cardForm.editingCardId, front, back, cardForm.hint, tags)
  } else {
    studyStore.createCard(selectedDeckId.value, front, back, cardForm.hint, tags)
  }

  resetCardForm()
}

function resetCardForm() {
  cardForm.editingCardId = ''
  cardForm.front = ''
  cardForm.back = ''
  cardForm.hint = ''
  cardForm.tagsText = ''
}

function startEditCard(cardId: string) {
  const card = deckCards.value.find((item) => item.id === cardId)
  if (!card) return

  cardForm.editingCardId = card.id
  cardForm.front = card.front
  cardForm.back = card.back
  cardForm.hint = card.hint ?? ''
  cardForm.tagsText = card.tags.join(', ')
}

function removeCard(cardId: string) {
  studyStore.removeCard(cardId)
  if (cardForm.editingCardId === cardId) {
    resetCardForm()
  }
}

function handleDeleteDeck() {
  if (!selectedDeckId.value) return

  const deletingDeckId = selectedDeckId.value
  studyStore.removeDeck(deletingDeckId)

  const nextDeck = studyStore.deckList[0]
  selectedDeckId.value = nextDeck?.id ?? ''
  resetCardForm()
}

function goToSession() {
  if (!selectedDeckId.value) return

  router.push({
    name: 'studySession',
    params: {
      deckId: selectedDeckId.value,
    },
  })
}

onMounted(() => {
  studyStore.load()

  if (studyStore.deckList.length > 0) {
    selectedDeckId.value = studyStore.deckList[0].id
  }
})

watch(selectedDeckId, () => {
  resetCardForm()
})
</script>

<style scoped>
.study-page {
  max-width: 1200px;
}

.study-header {
  border-left: 6px solid #0d6efd;
  padding-left: 1rem;
}

.metric-box {
  background: #f8f9fa;
  border-radius: 0.75rem;
  padding: 0.75rem 1rem;
  height: 100%;
}

.metric-number {
  font-size: 1.6rem;
  font-weight: 700;
  line-height: 1.1;
}

.metric-label {
  color: #6c757d;
  font-size: 0.9rem;
}

.card-item {
  border: 1px solid #e9ecef;
  border-radius: 0.75rem;
  background: #fff;
}

.active-deck {
  border: 1px solid #0d6efd !important;
  background: #f4f8ff !important;
}
</style>
