<template>
  <section class="manage-wrap">
    <header class="manage-header">
      <p class="eyebrow">Authenticated Workspace</p>
      <h1>Collection Manager</h1>
      <p>Build your own collections and cards, then move to focused study session.</p>
    </header>

    <div class="manage-grid">
      <aside class="panel">
        <h2>Create collection</h2>
        <form class="stack" @submit.prevent="handleCreateDeck">
          <input
            v-model="newDeck.title"
            class="form-control"
            type="text"
            placeholder="Collection name"
            required
          />
          <textarea
            v-model="newDeck.description"
            class="form-control"
            placeholder="Short description"
            rows="3"
          />
          <button class="btn btn-primary" type="submit">Create</button>
        </form>

        <hr />

        <h2>Your collections</h2>
        <div class="stack">
          <input
            v-model="deckQuery"
            class="form-control"
            type="search"
            placeholder="Search title or description"
          />
          <select v-model="deckSort" class="form-select">
            <option value="recent">Recently updated</option>
            <option value="title">Title A-Z</option>
            <option value="cards">Most cards</option>
          </select>
        </div>

        <p v-if="!filteredDeckList.length" class="empty-text">No matching collections.</p>
        <div v-else class="stack">
          <button
            v-for="deck in filteredDeckList"
            :key="deck.id"
            class="deck-tile"
            :class="{ active: selectedDeckId === deck.id }"
            @click="selectedDeckId = deck.id"
          >
            <strong>{{ deck.title }}</strong>
            <small>{{ getDeckStats(deck.id).totalCards }} cards</small>
          </button>
        </div>
      </aside>

      <main class="panel main-panel">
        <section v-if="selectedDeck">
          <div class="panel-head">
            <div>
              <h2 v-if="!isEditingDeck">{{ selectedDeck.title }}</h2>
              <p v-if="!isEditingDeck" class="text-muted">{{ selectedDeck.description || 'No description yet.' }}</p>
              <p v-else class="text-muted">Editing collection details</p>
            </div>
            <div class="actions-row">
              <button v-if="!isEditingDeck" class="btn btn-outline-primary btn-sm" @click="startEditDeck">Edit collection</button>
              <button class="btn btn-outline-danger btn-sm" @click="handleDeleteDeck">Delete</button>
              <button class="btn btn-success btn-sm" @click="goToSession">Open Session</button>
            </div>
          </div>

          <form v-if="isEditingDeck" class="deck-edit-form" @submit.prevent="handleUpdateDeck">
            <input
              v-model="deckEditForm.title"
              class="form-control"
              type="text"
              placeholder="Collection name"
              required
            />
            <textarea
              v-model="deckEditForm.description"
              class="form-control"
              rows="2"
              placeholder="Short description"
            />
            <div class="actions-row">
              <button class="btn btn-primary btn-sm" type="submit">Save collection</button>
              <button class="btn btn-outline-secondary btn-sm" type="button" @click="cancelEditDeck">Cancel</button>
            </div>
          </form>

          <div class="metrics">
            <article>
              <span>Total cards</span>
              <strong>{{ deckStats.totalCards }}</strong>
            </article>
            <article>
              <span>Due now</span>
              <strong>{{ deckStats.dueCards }}</strong>
            </article>
            <article>
              <span>Mastered</span>
              <strong>{{ deckStats.masteredCards }}</strong>
            </article>
          </div>

          <h3>Create or edit card</h3>
          <form class="card-form" @submit.prevent="handleSubmitCard">
            <input v-model="cardForm.front" class="form-control" type="text" placeholder="Front text" required />
            <input v-model="cardForm.back" class="form-control" type="text" placeholder="Back text" required />
            <input v-model="cardForm.hint" class="form-control" type="text" placeholder="Hint (optional)" />
            <input
              v-model="cardForm.tagsText"
              class="form-control"
              type="text"
              placeholder="Tags, comma separated"
            />
            <div class="actions-row">
              <button class="btn btn-primary" type="submit">
                {{ cardForm.editingCardId ? 'Save card' : 'Add card' }}
              </button>
              <button v-if="cardForm.editingCardId" class="btn btn-outline-secondary" type="button" @click="resetCardForm">
                Cancel
              </button>
            </div>
          </form>

          <h3>Quick import</h3>
          <textarea
            v-model="importText"
            class="form-control mb-2"
            rows="5"
            placeholder="Example:\ndeadline:a date when work must finish"
          />
          <div class="actions-row">
            <button class="btn btn-outline-primary" @click="handleImportCards">Import lines</button>
            <span v-if="importResult" class="text-muted small">{{ importResult }}</span>
          </div>
          <p class="small text-muted">Use one card per line. Front and back are separated by colon.</p>

          <h3>Cards</h3>
          <div class="card-controls">
            <input
              v-model="cardQuery"
              class="form-control"
              type="search"
              placeholder="Search front, back, hint, or tags"
            />
            <select v-model="cardFilter" class="form-select">
              <option value="all">All</option>
              <option value="due">Due</option>
              <option value="mastered">Mastered</option>
              <option value="new">New</option>
            </select>
            <select v-model="cardSort" class="form-select">
              <option value="dueSoon">Due soonest</option>
              <option value="frontAZ">Front A-Z</option>
              <option value="recentReview">Recently reviewed</option>
              <option value="streakDesc">Highest streak</option>
            </select>
          </div>

          <p v-if="!filteredDeckCards.length" class="empty-text">No cards match your current filters.</p>
          <div v-else class="stack">
            <article v-for="card in filteredDeckCards" :key="card.id" class="card-row">
              <div>
                <strong>{{ card.front }}</strong>
                <p>{{ card.back }}</p>
                <small v-if="card.hint">Hint: {{ card.hint }}</small>
                <div class="tags" v-if="card.tags.length">
                  <span v-for="tag in card.tags" :key="tag">{{ tag }}</span>
                </div>
              </div>
              <div class="actions-col">
                <button class="btn btn-outline-primary btn-sm" @click="startEditCard(card.id)">Edit</button>
                <button class="btn btn-outline-danger btn-sm" @click="removeCard(card.id)">Delete</button>
              </div>
            </article>
          </div>
        </section>

        <section v-else class="empty-center">Choose a collection to start managing cards.</section>
      </main>
    </div>
  </section>
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

const deckEditForm = reactive({
  title: '',
  description: '',
})
const isEditingDeck = ref(false)

const cardForm = reactive({
  editingCardId: '',
  front: '',
  back: '',
  hint: '',
  tagsText: '',
})

const importText = ref('')
const importResult = ref('')
const deckQuery = ref('')
const deckSort = ref<'recent' | 'title' | 'cards'>('recent')
const cardQuery = ref('')
const cardFilter = ref<'all' | 'due' | 'mastered' | 'new'>('all')
const cardSort = ref<'dueSoon' | 'frontAZ' | 'recentReview' | 'streakDesc'>('dueSoon')

const deckList = computed(() => studyStore.deckList)
const filteredDeckList = computed(() => {
  const query = deckQuery.value.trim().toLowerCase()
  const filtered = deckList.value.filter((deck) => {
    if (!query) return true
    return [deck.title, deck.description]
      .join(' ')
      .toLowerCase()
      .includes(query)
  })

  return [...filtered].sort((left, right) => {
    if (deckSort.value === 'title') {
      return left.title.localeCompare(right.title)
    }

    if (deckSort.value === 'cards') {
      return getDeckStats(right.id).totalCards - getDeckStats(left.id).totalCards
    }

    return right.updatedAt.localeCompare(left.updatedAt)
  })
})
const selectedDeck = computed(() => studyStore.getDeckById(selectedDeckId.value))
const deckCards = computed(() => {
  if (!selectedDeckId.value) return []
  return studyStore.getDeckCards(selectedDeckId.value)
})
const filteredDeckCards = computed(() => {
  const query = cardQuery.value.trim().toLowerCase()
  const now = Date.now()

  const filtered = deckCards.value.filter((card) => {
    const matchesQuery = !query || [card.front, card.back, card.hint ?? '', card.tags.join(' ')]
      .join(' ')
      .toLowerCase()
      .includes(query)

    if (!matchesQuery) {
      return false
    }

    if (cardFilter.value === 'due') {
      return new Date(card.dueAt).getTime() <= now
    }

    if (cardFilter.value === 'mastered') {
      return card.streak >= 5
    }

    if (cardFilter.value === 'new') {
      return card.streak === 0
    }

    return true
  })

  return [...filtered].sort((left, right) => {
    if (cardSort.value === 'frontAZ') {
      return left.front.localeCompare(right.front)
    }

    if (cardSort.value === 'recentReview') {
      return (right.lastReviewedAt ?? '').localeCompare(left.lastReviewedAt ?? '')
    }

    if (cardSort.value === 'streakDesc') {
      return right.streak - left.streak
    }

    return new Date(left.dueAt).getTime() - new Date(right.dueAt).getTime()
  })
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

function startEditDeck() {
  if (!selectedDeck.value) return

  isEditingDeck.value = true
  deckEditForm.title = selectedDeck.value.title
  deckEditForm.description = selectedDeck.value.description ?? ''
}

function cancelEditDeck() {
  isEditingDeck.value = false
  deckEditForm.title = ''
  deckEditForm.description = ''
}

async function handleUpdateDeck() {
  if (!selectedDeckId.value) return

  const title = deckEditForm.title.trim()
  if (!title) return

  const description = deckEditForm.description.trim()
  const updated = await studyStore.updateDeck(selectedDeckId.value, title, description)
  if (!updated) return

  cancelEditDeck()
}

async function handleCreateDeck() {
  const title = newDeck.title.trim()
  if (!title) return

  const deck = await studyStore.createDeck(title, newDeck.description)
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

async function handleSubmitCard() {
  if (!selectedDeckId.value) return

  const front = cardForm.front.trim()
  const back = cardForm.back.trim()
  if (!front || !back) return

  const tags = parseTags(cardForm.tagsText)

  if (cardForm.editingCardId) {
    await studyStore.updateCard(cardForm.editingCardId, front, back, cardForm.hint, tags)
  } else {
    await studyStore.createCard(selectedDeckId.value, front, back, cardForm.hint, tags)
  }

  resetCardForm()
}

async function handleImportCards() {
  if (!selectedDeckId.value) return
  if (!importText.value.trim()) return

  const result = await studyStore.importCardsFromText(selectedDeckId.value, importText.value)
  importResult.value = `Imported ${result.imported} card(s), skipped ${result.skipped} invalid line(s).`

  if (result.imported > 0) {
    importText.value = ''
  }
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

async function removeCard(cardId: string) {
  await studyStore.removeCard(cardId)
  if (cardForm.editingCardId === cardId) {
    resetCardForm()
  }
}

async function handleDeleteDeck() {
  if (!selectedDeckId.value) return

  const deletingDeckId = selectedDeckId.value
  await studyStore.removeDeck(deletingDeckId)

  const nextDeck = studyStore.deckList[0]
  selectedDeckId.value = nextDeck?.id ?? ''
  resetCardForm()
  cancelEditDeck()
}

function goToSession() {
  if (!selectedDeckId.value) return

  router.push({
    name: 'manageSession',
    params: {
      deckId: selectedDeckId.value,
    },
  })
}

onMounted(async () => {
  await studyStore.load()

  if (studyStore.deckList.length > 0) {
    selectedDeckId.value = studyStore.deckList[0].id
  }
})

watch(selectedDeckId, () => {
  resetCardForm()
  cancelEditDeck()
  cardQuery.value = ''
  cardFilter.value = 'all'
  cardSort.value = 'dueSoon'
})

watch(filteredDeckList, (nextDecks) => {
  if (!nextDecks.length) {
    selectedDeckId.value = ''
    return
  }

  if (!nextDecks.some((deck) => deck.id === selectedDeckId.value)) {
    selectedDeckId.value = nextDecks[0].id
  }
})
</script>

<style scoped>
.manage-wrap {
  max-width: 1120px;
  margin: 0 auto;
  padding: 2rem 1rem 3rem;
}

.eyebrow {
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.16em;
  font-size: 0.72rem;
  color: #705f4d;
}

.manage-header h1 {
  margin: 0.3rem 0;
}

.manage-header p {
  color: #4d5568;
}

.manage-grid {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 1rem;
}

.panel {
  background: #fff;
  border: 1px solid #e8e1d7;
  border-radius: 20px;
  padding: 1rem;
}

.panel h2,
.panel h3 {
  margin-top: 0;
}

.stack {
  display: grid;
  gap: 0.55rem;
}

.deck-tile {
  border: 1px solid #e3e8f3;
  border-radius: 12px;
  text-align: left;
  background: #f8fbff;
  padding: 0.6rem 0.7rem;
  display: flex;
  flex-direction: column;
}

.deck-tile.active {
  border-color: #1f4ed8;
  background: #edf3ff;
}

.deck-tile small {
  color: #6d7586;
}

.panel-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 0.8rem;
}

.actions-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.45rem;
  align-items: center;
}

.metrics {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.55rem;
  margin-bottom: 1rem;
}

.deck-edit-form {
  display: grid;
  gap: 0.55rem;
  margin-bottom: 1rem;
}

.metrics article {
  border: 1px solid #ece7df;
  border-radius: 12px;
  padding: 0.6rem;
  background: #fffaf2;
  display: flex;
  flex-direction: column;
}

.metrics span {
  color: #6f7583;
  font-size: 0.82rem;
}

.metrics strong {
  font-size: 1.25rem;
}

.card-form {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.55rem;
  margin-bottom: 1rem;
}

.card-form .actions-row {
  grid-column: 1 / -1;
}

.card-controls {
  display: grid;
  grid-template-columns: 1.4fr 0.9fr 1fr;
  gap: 0.55rem;
  margin-bottom: 0.8rem;
}

.card-row {
  border: 1px solid #e7e1d8;
  border-radius: 12px;
  padding: 0.75rem;
  background: #fff;
  display: flex;
  justify-content: space-between;
  gap: 0.6rem;
}

.card-row p {
  margin: 0.25rem 0;
  color: #4d566b;
}

.tags {
  margin-top: 0.35rem;
  display: flex;
  flex-wrap: wrap;
  gap: 0.3rem;
}

.tags span {
  border: 1px solid #dfe5f2;
  border-radius: 999px;
  padding: 0.1rem 0.42rem;
  font-size: 0.76rem;
  color: #47516a;
}

.actions-col {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.empty-text {
  color: #727b8e;
  margin-top: 0.75rem;
}

.empty-center {
  min-height: 220px;
  display: grid;
  place-items: center;
  color: #758095;
}

@media (max-width: 980px) {
  .manage-grid {
    grid-template-columns: 1fr;
  }

  .card-controls {
    grid-template-columns: 1fr;
  }

  .card-form {
    grid-template-columns: 1fr;
  }

  .metrics {
    grid-template-columns: 1fr;
  }
}
</style>
