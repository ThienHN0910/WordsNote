<template>
  <section class="manage-wrap">
    <header class="manage-header">
      <p class="eyebrow">Local-First Workspace</p>
      <h1>Collection Manager</h1>
      <p>Manage collections/cards locally without login. Sign in only when you need cloud-backed focused session APIs.</p>
      <p class="text-muted small mode-label">
        Mode: {{ isAuthenticated ? 'Cloud (authenticated)' : 'Local (no login)' }}
      </p>

      <section class="sync-panel">
        <h2>Cloud and Sync Quick Actions</h2>
        <p class="text-muted small">
          Without login, actions remain local. Use Sync Cloud -> Local to copy cloud data into browser storage.
          Use Sync Local -> Cloud after login.
        </p>
        <div class="actions-row">
          <button class="btn btn-outline-primary btn-sm" :disabled="isSyncing || isManageLoading" @click="handleSyncCloudToLocal">
            {{ isSyncing ? 'Syncing...' : 'Sync Cloud -> Local' }}
          </button>
          <button class="btn btn-primary btn-sm" :disabled="!canSyncLocalToCloud" @click="handleSyncLocalToCloud">
            {{ isSyncing ? 'Syncing...' : 'Sync Local -> Cloud' }}
          </button>
        </div>
        <p v-if="syncMessage" class="sync-note ok">{{ syncMessage }}</p>
        <p v-if="syncError" class="sync-note warn">{{ syncError }}</p>
      </section>
    </header>

    <div v-if="isManageLoading" class="manage-grid manage-grid-skeleton">
      <aside class="panel panel-skeleton">
        <AppSkeleton width="45%" height="20px" radius="10px" />
        <div class="stack" style="margin-top: 0.7rem;">
          <AppSkeleton height="38px" radius="10px" />
          <AppSkeleton height="78px" radius="10px" />
          <AppSkeleton width="34%" height="1px" radius="1px" />
          <AppSkeleton width="46%" height="20px" radius="10px" />
          <AppSkeleton height="38px" radius="10px" />
          <AppSkeleton height="38px" radius="10px" />
          <AppSkeleton v-for="slot in 5" :key="`deck-skeleton-${slot}`" height="56px" radius="12px" />
        </div>
      </aside>

      <main class="panel panel-skeleton main-panel">
        <AppSkeleton width="54%" height="30px" radius="10px" />
        <AppSkeleton width="72%" height="16px" radius="10px" />

        <div class="metrics metrics-skeleton">
          <article v-for="metric in 3" :key="`metric-skeleton-${metric}`">
            <AppSkeleton width="66%" height="12px" radius="10px" />
            <AppSkeleton width="40%" height="28px" radius="10px" />
          </article>
        </div>

        <AppSkeleton width="26%" height="22px" radius="10px" />
        <div class="stack" style="margin-top: 0.4rem;">
          <AppSkeleton height="42px" radius="10px" />
          <AppSkeleton height="42px" radius="10px" />
          <AppSkeleton height="42px" radius="10px" />
          <AppSkeleton height="42px" radius="10px" />
        </div>

        <AppSkeleton width="20%" height="22px" radius="10px" style="margin-top: 1rem;" />
        <AppSkeleton height="106px" radius="10px" style="margin-top: 0.4rem;" />

        <AppSkeleton width="18%" height="22px" radius="10px" style="margin-top: 1rem;" />
        <div class="stack" style="margin-top: 0.45rem;">
          <AppSkeleton v-for="row in 3" :key="`card-row-skeleton-${row}`" height="92px" radius="12px" />
        </div>
      </main>
    </div>

    <div v-else class="manage-grid">
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
                <button v-if="isAuthenticated" class="btn btn-success btn-sm" @click="goToSession">Open Session</button>
                <RouterLink v-else class="btn btn-outline-secondary btn-sm" :to="{ name: 'login', query: { redirect: '/manage' } }">
                  Login for Session
                </RouterLink>
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

          <h3>Create card</h3>
          <form class="card-form" @submit.prevent="handleSubmitCard">
            <input v-model="createCardForm.front" class="form-control" type="text" placeholder="Front text" required />
            <input v-model="createCardForm.back" class="form-control" type="text" placeholder="Back text" required />
            <input v-model="createCardForm.hint" class="form-control" type="text" placeholder="Hint (optional)" />
            <input
              v-model="createCardForm.tagsText"
              class="form-control"
              type="text"
              placeholder="Tags, comma separated"
            />
            <div class="actions-row">
              <button class="btn btn-primary" type="submit">Add card</button>
              <button class="btn btn-outline-secondary" type="button" @click="clearCreateCardForm">Clear</button>
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
                <button class="btn btn-outline-primary btn-sm" @click="openEditCardModal(card.id)">Edit</button>
                <button class="btn btn-outline-danger btn-sm" @click="removeCard(card.id)">Delete</button>
              </div>
            </article>
          </div>

          <Teleport to="body">
            <div v-if="editCardModal.isOpen" class="modal-backdrop" @click.self="closeEditCardModal">
              <section class="modal-card" role="dialog" aria-modal="true" aria-label="Edit card">
                <div class="modal-head">
                  <h3>Edit card</h3>
                  <button class="btn btn-outline-secondary btn-sm" type="button" @click="closeEditCardModal">Close</button>
                </div>
                <form class="modal-form" @submit.prevent="submitEditCard">
                  <input v-model="editCardModal.front" class="form-control" type="text" placeholder="Front text" required />
                  <input v-model="editCardModal.back" class="form-control" type="text" placeholder="Back text" required />
                  <input v-model="editCardModal.hint" class="form-control" type="text" placeholder="Hint (optional)" />
                  <input
                    v-model="editCardModal.tagsText"
                    class="form-control"
                    type="text"
                    placeholder="Tags, comma separated"
                  />
                  <div class="actions-row">
                    <button class="btn btn-primary" type="submit">Save changes</button>
                    <button class="btn btn-outline-secondary" type="button" @click="closeEditCardModal">Cancel</button>
                  </div>
                </form>
              </section>
            </div>
          </Teleport>
        </section>

        <section v-else class="empty-center">Choose a collection to start managing cards.</section>
      </main>
    </div>
  </section>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { useStudyStore } from '@/stores/WordsNote/StudyStore'
import AppSkeleton from '@/components/ui/AppSkeleton.vue'

const router = useRouter()
const authStore = useAuthStore()
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

const createCardForm = reactive({
  front: '',
  back: '',
  hint: '',
  tagsText: '',
})

const editCardModal = reactive({
  isOpen: false,
  cardId: '',
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
const isSyncing = ref(false)
const syncMessage = ref('')
const syncError = ref('')
const isManageLoading = ref(true)

const isAuthenticated = computed(() => authStore.isAuthenticated)
const canSyncLocalToCloud = computed(() => isAuthenticated.value && !isSyncing.value && !isManageLoading.value)

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

  const front = createCardForm.front.trim()
  const back = createCardForm.back.trim()
  if (!front || !back) return

  const tags = parseTags(createCardForm.tagsText)
  await studyStore.createCard(selectedDeckId.value, front, back, createCardForm.hint.trim(), tags)

  clearCreateCardForm()
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

async function handleSyncCloudToLocal() {
  syncError.value = ''
  syncMessage.value = ''
  isSyncing.value = true

  try {
    const currentDeckId = selectedDeckId.value
    const result = await studyStore.syncCloudToLocal()

    if (currentDeckId && studyStore.getDeckById(currentDeckId)) {
      selectedDeckId.value = currentDeckId
    } else {
      selectedDeckId.value = studyStore.deckList[0]?.id ?? ''
    }

    syncMessage.value = `Synced cloud -> local with ${result.deckCount} collection(s) and ${result.cardCount} card(s).`
  } catch (error) {
    const message = error instanceof Error ? error.message : 'Unable to sync cloud data to local.'
    syncError.value = message
  } finally {
    isSyncing.value = false
  }
}

async function handleSyncLocalToCloud() {
  syncError.value = ''
  syncMessage.value = ''

  if (!isAuthenticated.value) {
    syncError.value = 'Please login with Google first to sync local data to cloud.'
    return
  }

  isSyncing.value = true

  try {
    const result = await studyStore.syncLocalToCloud()
    syncMessage.value = `Synced ${result.deckCount} local collection(s): uploaded ${result.uploadedCards}, updated ${result.updatedCards}, skipped ${result.skippedCards} unchanged card(s).`
  } catch (error) {
    const message = error instanceof Error ? error.message : 'Unable to sync local data to cloud.'
    syncError.value = message
  } finally {
    isSyncing.value = false
  }
}

function clearCreateCardForm() {
  createCardForm.front = ''
  createCardForm.back = ''
  createCardForm.hint = ''
  createCardForm.tagsText = ''
}

function openEditCardModal(cardId: string) {
  const card = deckCards.value.find((item) => item.id === cardId)
  if (!card) return

  editCardModal.isOpen = true
  editCardModal.cardId = card.id
  editCardModal.front = card.front
  editCardModal.back = card.back
  editCardModal.hint = card.hint ?? ''
  editCardModal.tagsText = card.tags.join(', ')
}

function closeEditCardModal() {
  editCardModal.isOpen = false
  editCardModal.cardId = ''
  editCardModal.front = ''
  editCardModal.back = ''
  editCardModal.hint = ''
  editCardModal.tagsText = ''
}

async function submitEditCard() {
  if (!selectedDeckId.value || !editCardModal.cardId) return

  const front = editCardModal.front.trim()
  const back = editCardModal.back.trim()
  if (!front || !back) return

  const tags = parseTags(editCardModal.tagsText)
  await studyStore.updateCard(editCardModal.cardId, front, back, editCardModal.hint.trim(), tags)
  closeEditCardModal()
}

async function removeCard(cardId: string) {
  await studyStore.removeCard(cardId)
  if (editCardModal.cardId === cardId) {
    closeEditCardModal()
  }
}

async function handleDeleteDeck() {
  if (!selectedDeckId.value) return

  const deletingDeckId = selectedDeckId.value
  await studyStore.removeDeck(deletingDeckId)

  const nextDeck = studyStore.deckList[0]
  selectedDeckId.value = nextDeck?.id ?? ''
  clearCreateCardForm()
  closeEditCardModal()
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

async function reloadManageWorkspace(preferredDeckId?: string) {
  isManageLoading.value = true

  try {
    await studyStore.load()

    const targetDeckId = preferredDeckId ?? selectedDeckId.value
    if (targetDeckId && studyStore.getDeckById(targetDeckId)) {
      selectedDeckId.value = targetDeckId
      return
    }

    selectedDeckId.value = studyStore.deckList[0]?.id ?? ''
  } finally {
    isManageLoading.value = false
  }
}

onMounted(async () => {
  await reloadManageWorkspace()
})

watch(selectedDeckId, () => {
  clearCreateCardForm()
  closeEditCardModal()
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

watch(isAuthenticated, async () => {
  const currentDeckId = selectedDeckId.value
  await reloadManageWorkspace(currentDeckId)
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
  color: var(--wn-muted);
}

.manage-header h1 {
  margin: 0.3rem 0;
}

.manage-header p {
  color: var(--wn-muted);
}

.sync-panel {
  margin-top: 0.8rem;
  border: 1px solid var(--wn-border);
  border-radius: 14px;
  background: var(--wn-surface-soft);
  padding: 0.75rem;
}

.sync-panel h2 {
  margin: 0;
  font-size: 1rem;
}

.sync-note {
  margin: 0.55rem 0 0;
  font-size: 0.9rem;
}

.ok {
  color: #0f766e;
  font-weight: 600;
}

.warn {
  color: #b45309;
  font-weight: 600;
}

.manage-grid {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 1rem;
}

.manage-loader {
  margin-top: 1rem;
}

.manage-grid-skeleton {
  align-items: start;
}

.panel-skeleton {
  display: grid;
  gap: 0.5rem;
}

.metrics-skeleton article {
  display: grid;
  gap: 0.5rem;
}

.panel {
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 20px;
  padding: 1rem;
  box-shadow: var(--wn-shadow-soft);
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
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  text-align: left;
  background: var(--wn-surface-soft);
  padding: 0.6rem 0.7rem;
  display: flex;
  flex-direction: column;
}

.deck-tile.active {
  border-color: var(--wn-primary);
  background: var(--wn-primary-soft);
}

.deck-tile small {
  color: var(--wn-muted);
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
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  padding: 0.6rem;
  background: var(--wn-surface-soft);
  display: flex;
  flex-direction: column;
}

.metrics span {
  color: var(--wn-muted);
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
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  padding: 0.75rem;
  background: var(--wn-surface);
  display: flex;
  justify-content: space-between;
  gap: 0.6rem;
}

.card-row p {
  margin: 0.25rem 0;
  color: var(--wn-muted);
}

.tags {
  margin-top: 0.35rem;
  display: flex;
  flex-wrap: wrap;
  gap: 0.3rem;
}

.tags span {
  border: 1px solid var(--wn-border);
  border-radius: 999px;
  padding: 0.1rem 0.42rem;
  font-size: 0.76rem;
  color: var(--wn-muted);
}

.actions-col {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.5);
  display: grid;
  place-items: center;
  padding: 1rem;
  z-index: 1200;
}

.modal-card {
  width: min(620px, 96vw);
  background: var(--wn-surface);
  border: 1px solid var(--wn-border);
  border-radius: 16px;
  padding: 1rem;
  box-shadow: var(--wn-shadow-soft);
}

.modal-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.6rem;
  margin-bottom: 0.8rem;
}

.modal-head h3 {
  margin: 0;
}

.modal-form {
  display: grid;
  gap: 0.55rem;
}

.empty-text {
  color: var(--wn-muted);
  margin-top: 0.75rem;
}

.empty-center {
  min-height: 220px;
  display: grid;
  place-items: center;
  color: var(--wn-muted);
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

  .modal-card {
    width: 100%;
  }
}
</style>
