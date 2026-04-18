<template>
  <section class="manage-lab">
    <header class="top-row">
      <div>
        <p class="panel-title">Local Manage</p>
        <p class="muted">Collections and cards are stored only in browser local storage.</p>
      </div>
      <button class="ghost" @click="refresh">Refresh</button>
    </header>

    <p v-if="statusMsg" class="status">{{ statusMsg }}</p>
    <p v-if="errorMsg" class="error">{{ errorMsg }}</p>

    <div v-if="loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="loading-row" />
    </div>

    <template v-else>
      <section class="panel">
        <h3>Collections</h3>

        <select v-model="selectedCollectionId" class="field">
          <option v-for="item in collectionOptions" :key="item.id" :value="item.id">
            {{ item.label }}
          </option>
        </select>

        <input v-model="collectionDraft.title" class="field" type="text" placeholder="Collection title" />
        <textarea
          v-model="collectionDraft.description"
          class="field area"
          rows="2"
          placeholder="Collection description"
        />

        <div class="actions">
          <button @click="handleCreateCollection">Create</button>
          <button class="soft" :disabled="!selectedCollection" @click="handleSaveCollection">Save</button>
          <button class="danger" :disabled="!canDeleteSelectedCollection" @click="handleDeleteCollection">Delete</button>
        </div>

        <p v-if="selectedCollection && selectedCollection.isSystem" class="muted">
          System collection cannot be deleted.
        </p>
      </section>

      <section class="panel">
        <h3>{{ editingCardId ? 'Edit Card' : 'Add Card' }}</h3>

        <input v-model="cardDraft.front" class="field" type="text" placeholder="Front (required)" />
        <input v-model="cardDraft.back" class="field" type="text" placeholder="Back" />
        <input v-model="cardDraft.hint" class="field" type="text" placeholder="Hint" />

        <div class="actions">
          <button @click="handleUpsertCard">{{ editingCardId ? 'Update Card' : 'Add Card' }}</button>
          <button v-if="editingCardId" class="soft" @click="resetCardDraft">Cancel</button>
        </div>

        <textarea
          v-model="importText"
          class="field area"
          rows="3"
          placeholder="Import lines: front|back|hint"
        />
        <button class="ghost" @click="handleImport">Import Lines</button>
      </section>

      <section class="panel">
        <h3>Cards ({{ filteredCards.length }})</h3>

        <p v-if="filteredCards.length === 0" class="muted">
          No cards in this collection yet.
        </p>

        <ul v-else class="card-list">
          <li v-for="card in filteredCards" :key="card.id" class="card-item">
            <div>
              <p class="card-front">{{ card.front }}</p>
              <p class="muted">{{ card.back || 'No back text' }}</p>
              <p v-if="card.hint" class="muted">Hint: {{ card.hint }}</p>
            </div>
            <div class="card-actions">
              <button class="soft" @click="startEditCard(card)">Edit</button>
              <button class="danger" @click="handleDeleteCard(card.id)">Delete</button>
            </div>
          </li>
        </ul>
      </section>
    </template>
  </section>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue';
import {
  createLocalCard,
  createLocalCollection,
  deleteLocalCard,
  deleteLocalCollection,
  getLocalCards,
  getLocalCollections,
  updateLocalCard,
  updateLocalCollection,
} from '../../services/storage.js';

const loading = ref(true);
const errorMsg = ref('');
const statusMsg = ref('');

const collections = ref([]);
const cards = ref([]);
const selectedCollectionId = ref('');
const importText = ref('');

const collectionDraft = reactive({
  title: '',
  description: '',
});

const cardDraft = reactive({
  front: '',
  back: '',
  hint: '',
});

const editingCardId = ref('');

const selectedCollection = computed(() =>
  collections.value.find((item) => item.id === selectedCollectionId.value) || null,
);

const canDeleteSelectedCollection = computed(() =>
  Boolean(selectedCollection.value && !selectedCollection.value.isSystem),
);

const filteredCards = computed(() => {
  const targetId = selectedCollectionId.value;
  if (!targetId) {
    return cards.value;
  }

  return cards.value.filter((card) => card.collectionId === targetId);
});

const collectionOptions = computed(() =>
  collections.value.map((collection) => {
    const count = cards.value.filter((card) => card.collectionId === collection.id).length;
    return {
      id: collection.id,
      label: `${collection.title} (${count})`,
    };
  }),
);

function resetCardDraft() {
  editingCardId.value = '';
  cardDraft.front = '';
  cardDraft.back = '';
  cardDraft.hint = '';
}

function syncCollectionDraft() {
  if (!selectedCollection.value) {
    collectionDraft.title = '';
    collectionDraft.description = '';
    return;
  }

  collectionDraft.title = selectedCollection.value.title || '';
  collectionDraft.description = selectedCollection.value.description || '';
}

function clearMessages() {
  errorMsg.value = '';
  statusMsg.value = '';
}

async function refresh() {
  loading.value = true;
  clearMessages();

  try {
    const [nextCollections, nextCards] = await Promise.all([getLocalCollections(), getLocalCards()]);
    collections.value = nextCollections;
    cards.value = nextCards;

    if (!selectedCollectionId.value || !collections.value.some((item) => item.id === selectedCollectionId.value)) {
      selectedCollectionId.value = collections.value[0]?.id || '';
    }

    syncCollectionDraft();
  } catch (error) {
    const detail = error instanceof Error ? error.message : '';
    errorMsg.value = `Failed to load local manage data.${detail ? ` ${detail}` : ''}`;
  } finally {
    loading.value = false;
  }
}

async function handleCreateCollection() {
  clearMessages();
  const created = await createLocalCollection(collectionDraft.title, collectionDraft.description);
  if (!created) {
    errorMsg.value = 'Collection title is required.';
    return;
  }

  await refresh();
  selectedCollectionId.value = created.id;
  syncCollectionDraft();
  statusMsg.value = 'Collection created.';
}

async function handleSaveCollection() {
  if (!selectedCollection.value) {
    return;
  }

  clearMessages();
  const updated = await updateLocalCollection(selectedCollection.value.id, {
    title: collectionDraft.title,
    description: collectionDraft.description,
  });

  if (!updated) {
    errorMsg.value = 'Cannot save collection. Title is required.';
    return;
  }

  await refresh();
  selectedCollectionId.value = updated.id;
  syncCollectionDraft();
  statusMsg.value = 'Collection saved.';
}

async function handleDeleteCollection() {
  if (!canDeleteSelectedCollection.value || !selectedCollection.value) {
    return;
  }

  const confirmed = window.confirm(
    `Delete collection "${selectedCollection.value.title}" and all its cards?`,
  );
  if (!confirmed) {
    return;
  }

  clearMessages();
  const deleted = await deleteLocalCollection(selectedCollection.value.id);
  if (!deleted) {
    errorMsg.value = 'Failed to delete collection.';
    return;
  }

  await refresh();
  resetCardDraft();
  statusMsg.value = 'Collection deleted.';
}

function startEditCard(card) {
  editingCardId.value = card.id;
  cardDraft.front = card.front || '';
  cardDraft.back = card.back || '';
  cardDraft.hint = card.hint || '';
}

async function handleUpsertCard() {
  clearMessages();

  if (!selectedCollection.value) {
    errorMsg.value = 'Select a collection first.';
    return;
  }

  if (!cardDraft.front.trim()) {
    errorMsg.value = 'Card front is required.';
    return;
  }

  if (editingCardId.value) {
    const updated = await updateLocalCard(editingCardId.value, {
      front: cardDraft.front,
      back: cardDraft.back,
      hint: cardDraft.hint,
      collectionId: selectedCollection.value.id,
    });

    if (!updated) {
      errorMsg.value = 'Failed to update card.';
      return;
    }

    await refresh();
    resetCardDraft();
    statusMsg.value = 'Card updated.';
    return;
  }

  const created = await createLocalCard({
    collectionId: selectedCollection.value.id,
    front: cardDraft.front,
    back: cardDraft.back,
    hint: cardDraft.hint,
  });

  if (!created) {
    errorMsg.value = 'Failed to add card.';
    return;
  }

  await refresh();
  resetCardDraft();
  statusMsg.value = 'Card added.';
}

async function handleDeleteCard(cardId) {
  const confirmed = window.confirm('Delete this card?');
  if (!confirmed) {
    return;
  }

  clearMessages();
  const deleted = await deleteLocalCard(cardId);
  if (!deleted) {
    errorMsg.value = 'Failed to delete card.';
    return;
  }

  await refresh();
  if (editingCardId.value === cardId) {
    resetCardDraft();
  }
  statusMsg.value = 'Card deleted.';
}

async function handleImport() {
  clearMessages();

  if (!selectedCollection.value) {
    errorMsg.value = 'Select a collection before importing.';
    return;
  }

  const lines = importText.value
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter((line) => line.length > 0);

  if (lines.length === 0) {
    errorMsg.value = 'Paste at least one import line.';
    return;
  }

  let imported = 0;
  let skipped = 0;

  for (const line of lines) {
    const [frontRaw, backRaw = '', hintRaw = ''] = line.split('|');
    const front = String(frontRaw || '').trim();
    if (!front) {
      skipped += 1;
      continue;
    }

    const created = await createLocalCard({
      collectionId: selectedCollection.value.id,
      front,
      back: String(backRaw || '').trim(),
      hint: String(hintRaw || '').trim(),
    });

    if (created) {
      imported += 1;
    } else {
      skipped += 1;
    }
  }

  await refresh();
  importText.value = '';
  statusMsg.value = `Imported ${imported} card${imported === 1 ? '' : 's'}${skipped > 0 ? `, skipped ${skipped}` : ''}.`;
}

watch(selectedCollectionId, () => {
  syncCollectionDraft();
  resetCardDraft();
});

onMounted(async () => {
  await refresh();
});
</script>

<style scoped>
.manage-lab {
  display: grid;
  gap: 10px;
}

.top-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
}

.panel-title {
  font-size: 15px;
  font-weight: 700;
}

.panel {
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  padding: 10px;
  display: grid;
  gap: 8px;
  background: var(--wn-surface-soft);
}

.panel h3 {
  font-size: 14px;
  color: var(--wn-ink);
  font-family: 'Fraunces', 'Georgia', serif;
}

.field {
  width: 100%;
  border: 1px solid var(--wn-border);
  border-radius: 10px;
  padding: 8px 10px;
  font-size: 12px;
  color: var(--wn-ink);
  background: var(--wn-surface);
}

.area {
  resize: vertical;
  min-height: 56px;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.actions button,
.ghost,
.soft,
.danger,
.card-actions button {
  border: 1px solid var(--wn-border);
  background: var(--wn-surface);
  border-radius: 10px;
  padding: 6px 10px;
  font-size: 12px;
  cursor: pointer;
  color: var(--wn-ink);
}

.actions button:not(.soft):not(.danger) {
  border-color: var(--wn-primary);
  background: var(--wn-primary);
  color: var(--wn-on-primary);
}

.soft {
  background: var(--wn-primary-soft);
  border-color: color-mix(in srgb, var(--wn-primary) 36%, var(--wn-border));
}

.ghost {
  background: color-mix(in srgb, var(--wn-accent) 16%, var(--wn-surface));
  border-color: color-mix(in srgb, var(--wn-accent) 44%, var(--wn-border));
}

.danger {
  background: #fff4f4;
  border-color: #f1c5c5;
  color: #9f1239;
}

.actions button:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.status {
  border: 1px solid #b6e3c4;
  background: #ecfff3;
  color: #166534;
  border-radius: 10px;
  padding: 8px 10px;
  font-size: 12px;
}

.error {
  border: 1px solid #f1b3b3;
  background: #fff3f3;
  color: #b42318;
  border-radius: 10px;
  padding: 8px 10px;
  font-size: 12px;
}

.muted {
  margin-top: 2px;
  font-size: 12px;
  color: var(--wn-muted);
}

.card-list {
  list-style: none;
  display: grid;
  gap: 8px;
}

.card-item {
  border: 1px dashed var(--wn-border);
  border-radius: 10px;
  padding: 8px;
  display: grid;
  gap: 8px;
}

.card-front {
  font-size: 13px;
  font-weight: 600;
}

.card-actions {
  display: flex;
  gap: 6px;
}

.loading-list {
  display: grid;
  gap: 8px;
}

.loading-row {
  height: 34px;
  border-radius: 8px;
  background: color-mix(in srgb, var(--wn-primary) 10%, var(--wn-surface));
  animation: pulse 1.4s infinite;
}

@keyframes pulse {
  0%,
  100% {
    opacity: 1;
  }

  50% {
    opacity: 0.4;
  }
}
</style>
