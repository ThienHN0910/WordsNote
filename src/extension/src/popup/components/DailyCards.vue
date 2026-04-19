<template>
  <section class="learn-lab">
    <header class="top-row">
      <div>
        <p class="due-count">
          {{ loading ? loadingLabel : countLabel }}
        </p>
        <p class="muted">{{ sourceDescription }}</p>
      </div>
      <button class="ghost" @click="refresh">Refresh</button>
    </header>

    <section class="source-config">
      <div class="source-switch">
        <button :class="{ active: sourceMode === 'local' }" @click="sourceMode = 'local'">Local Due</button>
        <button :class="{ active: sourceMode === 'cloud' }" @click="sourceMode = 'cloud'">Cloud Sync</button>
      </div>

      <div v-if="sourceMode === 'cloud'" class="cloud-controls">
        <input
          v-model="cloudApiBaseDraft"
          class="cloud-input"
          type="text"
          placeholder="http://words-note.runasp.net"
          @keyup.enter="saveCloudEndpoint"
        />
        <button class="ghost" @click="saveCloudEndpoint">Save Endpoint</button>
        <input
          v-model="cloudAuthTokenDraft"
          class="cloud-input"
          type="password"
          placeholder="Paste cloud JWT token"
          @keyup.enter="saveCloudAuth"
        />
        <button class="ghost" @click="saveCloudAuth">Save Token</button>
        <button class="ghost" @click="syncCloudToLocal">Sync To Local</button>
        <button class="ghost" :disabled="!isCloudAuthenticated" @click="syncLocalToCloud">Sync Local -> Cloud</button>
      </div>

      <p v-if="sourceMode === 'cloud'" class="muted cloud-note">
        Endpoint: {{ cloudApiBaseUrl }}. Cloud write mode {{ isCloudAuthenticated ? 'enabled' : 'requires token' }}.
      </p>
      <p v-if="syncSummary" class="sync-summary">{{ syncSummary }}</p>
    </section>

    <section v-if="collectionOptions.length > 1" class="collection-filter">
      <label class="collection-label" for="collection-select">Collection</label>
      <select id="collection-select" v-model="selectedCollectionId" class="collection-select">
        <option v-for="option in collectionOptions" :key="option.id" :value="option.id">
          {{ option.label }}
        </option>
      </select>
    </section>

    <p v-if="errorMsg" class="error">{{ errorMsg }}</p>

    <div v-if="loading" class="loading-list">
      <div v-for="i in 3" :key="i" class="loading-row" />
    </div>

    <div v-else-if="!errorMsg && cards.length === 0" class="empty-state">
      <p class="empty-title">{{ emptyTitle }}</p>
      <p class="muted">{{ emptyDescription }}</p>
    </div>

    <div v-else class="mode-shell">
      <div class="mode-switch">
        <button :class="{ active: mode === 'flash' }" @click="mode = 'flash'">Flashcards</button>
        <button :class="{ active: mode === 'learn' }" @click="mode = 'learn'">Learn</button>
        <button :class="{ active: mode === 'practice' }" @click="mode = 'practice'">Practice</button>
      </div>

      <p class="progress">Card {{ currentIndex + 1 }} / {{ cards.length }}</p>

      <section class="card-stage">
        <template v-if="mode === 'flash'">
          <div class="flash-card" :class="{ flipped: showBack }" @click="showBack = !showBack">
            <div class="flash-card-inner">
              <article class="flash-face flash-front">
                <p class="face-label">Front</p>
                <h3>{{ currentCard.front }}</h3>
                <small v-if="currentCard.hint">Hint: {{ currentCard.hint }}</small>
                <small v-if="currentCard.collectionTitle" class="card-meta">Collection: {{ currentCard.collectionTitle }}</small>
              </article>
              <article class="flash-face flash-back">
                <p class="face-label">Back</p>
                <h3>{{ resolvedBack }}</h3>
              </article>
            </div>
          </div>

          <div class="actions">
            <button class="ghost" @click="previousCard">Previous</button>
            <button class="soft" @click="showBack = !showBack">{{ showBack ? 'Show front' : 'Flip' }}</button>
            <button @click="nextCard">Next</button>
          </div>
        </template>

        <template v-else-if="mode === 'learn'">
          <h3 class="prompt">{{ currentCard.front }}</h3>
          <input
            v-model="typedAnswer"
            class="answer-input"
            type="text"
            placeholder="Type your answer"
            @keyup.enter="checkLearn"
          />

          <div class="actions">
            <button @click="checkLearn">{{ learnAwaitingNext ? 'Next card' : 'Check' }}</button>
            <button class="soft" @click="nextCard">Skip</button>
          </div>

          <p v-if="learnFeedback" :class="learnIsCorrect ? 'ok' : 'warn'">{{ learnFeedback }}</p>
        </template>

        <template v-else>
          <h3 class="prompt">{{ currentCard.front }}</h3>

          <div class="option-grid">
            <button
              v-for="option in practiceQuestion.options"
              :key="option"
              class="option"
              :class="{
                selected: selectedPracticeOption === option,
                correct: practiceAwaitingNext && option === practiceQuestion.answer,
                wrong:
                  practiceAwaitingNext &&
                  selectedPracticeOption === option &&
                  option !== practiceQuestion.answer,
              }"
              :disabled="practiceAwaitingNext"
              @click="answerPractice(option)"
            >
              {{ option }}
            </button>
          </div>

          <div v-if="practiceAwaitingNext" class="actions">
            <button @click="nextCard">Next question</button>
          </div>

          <p v-if="practiceFeedback" :class="practiceFeedbackType">{{ practiceFeedback }}</p>
        </template>
      </section>

      <footer class="review-footer">
        <p class="muted">{{ reviewHint }}</p>
        <div v-if="sourceMode === 'local'" class="difficulty-actions">
          <button class="difficulty hard" @click="reviewCurrent('hard')">Hard</button>
          <button class="difficulty medium" @click="reviewCurrent('medium')">Medium</button>
          <button class="difficulty easy" @click="reviewCurrent('easy')">Easy</button>
        </div>
      </footer>
    </div>
  </section>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import {
  getDueLocalCards,
  getLocalCards,
  getLocalCollections,
  reviewLocalCard,
  syncCloudCardsToLocal,
} from '../../services/storage.js';
import {
  fetchPublicLearnSnapshot,
  getCloudAuthToken,
  getCloudApiBaseUrl,
  setCloudAuthToken,
  setCloudApiBaseUrl,
  syncLocalSnapshotToCloud,
} from '../../services/remoteStudy.js';

const allCards = ref([]);
const loading = ref(true);
const errorMsg = ref('');
const syncSummary = ref('');
const selectedCollectionId = ref('all');

const sourceMode = ref('local');
const cloudApiBaseUrl = ref('http://words-note.runasp.net');
const cloudApiBaseDraft = ref('http://words-note.runasp.net');
const cloudAuthToken = ref('');
const cloudAuthTokenDraft = ref('');

const mode = ref('flash');
const currentIndex = ref(0);
const showBack = ref(false);

const typedAnswer = ref('');
const learnFeedback = ref('');
const learnIsCorrect = ref(false);
const learnAwaitingNext = ref(false);

const practiceOptions = ref([]);
const selectedPracticeOption = ref('');
const practiceFeedback = ref('');
const practiceFeedbackType = ref('');
const practiceAwaitingNext = ref(false);

const cards = computed(() => {
  if (selectedCollectionId.value === 'all') {
    return allCards.value;
  }

  return allCards.value.filter((card) => card.collectionId === selectedCollectionId.value);
});

const collectionOptions = computed(() => {
  const optionMap = new Map();

  for (const card of allCards.value) {
    const collectionId = String(card.collectionId || 'local-inbox').trim() || 'local-inbox';
    const collectionTitle = String(card.collectionTitle || 'Local Inbox').trim() || 'Local Inbox';

    if (!optionMap.has(collectionId)) {
      optionMap.set(collectionId, collectionTitle);
    }
  }

  const sortedOptions = [...optionMap.entries()]
    .sort((a, b) => a[1].localeCompare(b[1]))
    .map(([id, title]) => ({ id, label: title }));

  return [{ id: 'all', label: 'All collections' }, ...sortedOptions];
});

const loadingLabel = computed(() =>
  sourceMode.value === 'local' ? 'Loading due cards...' : 'Syncing from public API...',
);

const countLabel = computed(() => {
  if (sourceMode.value === 'local') {
    return `${cards.value.length} due card${cards.value.length === 1 ? '' : 's'}`;
  }

  return `${cards.value.length} synced card${cards.value.length === 1 ? '' : 's'}`;
});

const sourceDescription = computed(() =>
  sourceMode.value === 'local'
    ? 'Learn-only popup, no login required'
    : isCloudAuthenticated.value
      ? 'Cloud mode can read public data and sync local changes to cloud'
      : 'Cloud mode reads public data. Add token to enable local -> cloud sync',
);

const isCloudAuthenticated = computed(() => cloudAuthToken.value.length > 0);

const emptyTitle = computed(() =>
  sourceMode.value === 'local' ? 'No cards due right now.' : 'No public cards found.',
);

const emptyDescription = computed(() =>
  sourceMode.value === 'local'
    ? 'Highlight text on any page and save it to build your next review set.'
    : 'Check your API endpoint and ensure public collections/cards have data.',
);

const reviewHint = computed(() =>
  sourceMode.value === 'local'
    ? 'Rate this card for spaced repetition:'
    : 'Cloud sync is read-only. Review scores are disabled in this mode.',
);

const currentCard = computed(() => {
  if (cards.value.length === 0) {
    return null;
  }

  const safeIndex = currentIndex.value % cards.value.length;
  return cards.value[safeIndex] || null;
});

const resolvedBack = computed(() => {
  if (!currentCard.value) {
    return '';
  }

  const rawBack = String(currentCard.value.back || '').trim();
  return rawBack || String(currentCard.value.front || '').trim();
});

const practiceQuestion = computed(() => ({
  prompt: currentCard.value ? String(currentCard.value.front || '') : '',
  answer: resolvedBack.value,
  options: practiceOptions.value,
}));

function shuffle(values) {
  const output = [...values];
  for (let i = output.length - 1; i > 0; i -= 1) {
    const j = Math.floor(Math.random() * (i + 1));
    const temp = output[i];
    output[i] = output[j];
    output[j] = temp;
  }
  return output;
}

function normalizeCard(card, fallbackCollectionId = 'local-inbox', fallbackCollectionTitle = 'Local Inbox') {
  const collectionId = String(card.collectionId || fallbackCollectionId).trim() || fallbackCollectionId;
  const collectionTitle = String(card.collectionTitle || fallbackCollectionTitle).trim() || fallbackCollectionTitle;

  return {
    ...card,
    collectionId,
    collectionTitle,
  };
}

function normalize(value) {
  return String(value || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim()
    .replace(/\s+/g, ' ');
}

function resetModeState() {
  showBack.value = false;

  typedAnswer.value = '';
  learnFeedback.value = '';
  learnIsCorrect.value = false;
  learnAwaitingNext.value = false;

  selectedPracticeOption.value = '';
  practiceFeedback.value = '';
  practiceFeedbackType.value = '';
  practiceAwaitingNext.value = false;
}

function rebuildPracticeOptions() {
  if (!currentCard.value) {
    practiceOptions.value = [];
    return;
  }

  const distractors = cards.value
    .filter((card) => card.id !== currentCard.value.id)
    .map((card) => String(card.back || '').trim() || String(card.front || '').trim())
    .filter((value) => value.length > 0);

  const options = [resolvedBack.value, ...shuffle(distractors)]
    .filter((value) => value.length > 0)
    .slice(0, 4);

  practiceOptions.value = shuffle([...new Set(options)]);
}

function nextCard() {
  if (cards.value.length === 0) {
    return;
  }

  resetModeState();
  currentIndex.value = (currentIndex.value + 1) % cards.value.length;
}

function previousCard() {
  if (cards.value.length === 0) {
    return;
  }

  resetModeState();
  currentIndex.value = (currentIndex.value - 1 + cards.value.length) % cards.value.length;
}

async function refresh() {
  loading.value = true;
  errorMsg.value = '';

  try {
    if (sourceMode.value === 'local') {
      const localDueCards = await getDueLocalCards();
      allCards.value = localDueCards.map((card) => normalizeCard(card, 'local-inbox', 'Local Inbox'));
    } else {
      const snapshot = await fetchPublicLearnSnapshot(cloudApiBaseUrl.value);
      allCards.value = snapshot.cards.map((card) =>
        normalizeCard(card, 'cloud-unknown', 'Cloud Collection'),
      );
      syncSummary.value = `${snapshot.collections} collection${snapshot.collections === 1 ? '' : 's'} synced.`;
    }

    if (
      selectedCollectionId.value !== 'all' &&
      !allCards.value.some((card) => card.collectionId === selectedCollectionId.value)
    ) {
      selectedCollectionId.value = 'all';
    }

    if (currentIndex.value >= cards.value.length) {
      currentIndex.value = 0;
    }

    resetModeState();
  } catch (error) {
    const detail = error instanceof Error ? error.message : '';
    errorMsg.value = sourceMode.value === 'local'
      ? 'Failed to load local cards.'
      : `Failed to sync public cards.${detail ? ` ${detail}` : ''}`;
  } finally {
    loading.value = false;
  }
}

async function reviewCurrent(difficulty) {
  if (sourceMode.value !== 'local') {
    return;
  }

  if (!currentCard.value) {
    return;
  }

  await reviewLocalCard(currentCard.value.id, difficulty);
  await refresh();
}

function checkLearn() {
  if (!currentCard.value) {
    return;
  }

  if (learnAwaitingNext.value) {
    nextCard();
    return;
  }

  const correct = normalize(typedAnswer.value) === normalize(resolvedBack.value);
  learnIsCorrect.value = correct;

  if (correct) {
    learnFeedback.value = 'Correct. Press Enter or Check for next card.';
    learnAwaitingNext.value = true;
    return;
  }

  learnFeedback.value = `Not yet. Expected answer: ${resolvedBack.value}`;
  learnAwaitingNext.value = true;
}

function answerPractice(option) {
  if (!currentCard.value || practiceAwaitingNext.value) {
    return;
  }

  const correct = option === practiceQuestion.value.answer;
  selectedPracticeOption.value = option;
  practiceAwaitingNext.value = true;
  practiceFeedbackType.value = correct ? 'ok' : 'warn';
  practiceFeedback.value = correct
    ? 'Correct answer.'
    : `Wrong. Correct answer: ${practiceQuestion.value.answer}`;
}

async function saveCloudEndpoint() {
  cloudApiBaseUrl.value = await setCloudApiBaseUrl(cloudApiBaseDraft.value);
  cloudApiBaseDraft.value = cloudApiBaseUrl.value;

  if (sourceMode.value === 'cloud') {
    currentIndex.value = 0;
    await refresh();
  }
}

async function saveCloudAuth() {
  cloudAuthToken.value = await setCloudAuthToken(cloudAuthTokenDraft.value);
  cloudAuthTokenDraft.value = cloudAuthToken.value;
  syncSummary.value = cloudAuthToken.value
    ? 'Cloud token saved. Local -> cloud sync is enabled.'
    : 'Cloud token cleared. Local -> cloud sync is disabled.';
}

async function syncCloudToLocal() {
  loading.value = true;
  errorMsg.value = '';

  try {
    const snapshot = await fetchPublicLearnSnapshot(cloudApiBaseUrl.value);
    const normalizedCloudCards = snapshot.cards.map((card) =>
      normalizeCard(card, 'cloud-unknown', 'Cloud Collection'),
    );
    const cardsToSync = selectedCollectionId.value === 'all'
      ? normalizedCloudCards
      : normalizedCloudCards.filter((card) => card.collectionId === selectedCollectionId.value);

    const syncResult = await syncCloudCardsToLocal(cardsToSync);

    sourceMode.value = 'local';
    selectedCollectionId.value = 'all';
    await refresh();
    syncSummary.value = `Synced ${syncResult.synced} card${syncResult.synced === 1 ? '' : 's'} from cloud to local.`;
  } catch (error) {
    const detail = error instanceof Error ? error.message : '';
    errorMsg.value = `Cloud sync to local failed.${detail ? ` ${detail}` : ''}`;
  } finally {
    loading.value = false;
  }
}

async function syncLocalToCloud() {
  loading.value = true;
  errorMsg.value = '';

  try {
    if (!isCloudAuthenticated.value) {
      throw new Error('Save cloud JWT token first.');
    }

    const [collections, cards] = await Promise.all([getLocalCollections(), getLocalCards()]);
    const result = await syncLocalSnapshotToCloud({
      baseUrl: cloudApiBaseUrl.value,
      token: cloudAuthToken.value,
      collections,
      cards,
    });

    syncSummary.value = `Local -> cloud done: uploaded ${result.uploadedCards}, updated ${result.updatedCards}, skipped ${result.skippedCards}.`;
  } catch (error) {
    const detail = error instanceof Error ? error.message : '';
    errorMsg.value = `Local to cloud sync failed.${detail ? ` ${detail}` : ''}`;
  } finally {
    loading.value = false;
  }
}

watch(mode, () => {
  resetModeState();
});

watch(sourceMode, async () => {
  currentIndex.value = 0;
  selectedCollectionId.value = 'all';
  await refresh();
});

watch(selectedCollectionId, () => {
  currentIndex.value = 0;
  resetModeState();
});

watch(
  () => currentCard.value && currentCard.value.id,
  () => {
    resetModeState();
    rebuildPracticeOptions();
  },
  { immediate: true },
);

onMounted(async () => {
  cloudApiBaseUrl.value = await getCloudApiBaseUrl();
  cloudApiBaseDraft.value = cloudApiBaseUrl.value;
  cloudAuthToken.value = await getCloudAuthToken();
  cloudAuthTokenDraft.value = cloudAuthToken.value;
  await refresh();
});
</script>

<style scoped>
.learn-lab {
  display: grid;
  gap: 10px;
}

.top-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
}

.source-config {
  display: grid;
  gap: 7px;
}

.collection-filter {
  display: grid;
  gap: 6px;
}

.collection-label {
  font-size: 12px;
  font-weight: 600;
  color: var(--wn-muted);
}

.collection-select {
  width: 100%;
  border: 1px solid var(--wn-border);
  background: var(--wn-surface);
  border-radius: 10px;
  padding: 8px 10px;
  font-size: 12px;
  color: var(--wn-ink);
}

.source-switch {
  display: flex;
  gap: 6px;
}

.source-switch button {
  flex: 1;
  border: 1px solid var(--wn-border);
  background: var(--wn-surface);
  border-radius: 10px;
  padding: 6px 8px;
  font-size: 12px;
  color: var(--wn-ink);
  cursor: pointer;
}

.source-switch button.active {
  border-color: var(--wn-primary);
  background: var(--wn-primary-soft);
  color: var(--wn-primary);
  font-weight: 600;
}

.cloud-controls {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.cloud-input {
  flex: 1;
  min-width: 180px;
  border: 1px solid var(--wn-border);
  border-radius: 10px;
  padding: 8px 10px;
  font-size: 12px;
  color: var(--wn-ink);
  background: var(--wn-surface);
}

.cloud-note {
  margin-top: 0;
}

.sync-summary {
  font-size: 12px;
  color: var(--wn-primary);
  font-weight: 600;
}

.due-count {
  font-size: 15px;
  font-weight: 700;
  color: var(--wn-ink);
}

.muted {
  margin-top: 2px;
  font-size: 12px;
  color: var(--wn-muted);
}

.error {
  border: 1px solid #f1b3b3;
  background: #fff3f3;
  color: #b42318;
  border-radius: 10px;
  padding: 10px 11px;
  font-size: 13px;
}

.ghost,
.soft,
.mode-switch button,
.actions button,
.option,
.difficulty {
  border: 1px solid var(--wn-border);
  background: var(--wn-surface);
  border-radius: 10px;
  padding: 6px 10px;
  font-size: 12px;
  cursor: pointer;
  color: var(--wn-ink);
  transition: all 0.2s ease;
}

.ghost {
  background: color-mix(in srgb, var(--wn-accent) 16%, var(--wn-surface));
  border-color: color-mix(in srgb, var(--wn-accent) 44%, var(--wn-border));
}

.ghost:hover,
.soft:hover,
.option:hover,
.difficulty:hover,
.mode-switch button:hover,
.actions button:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(31, 35, 51, 0.1);
}

.mode-shell {
  display: grid;
  gap: 10px;
}

.mode-switch {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.mode-switch button.active {
  border-color: var(--wn-primary);
  background: var(--wn-primary);
  color: var(--wn-on-primary);
}

.progress {
  font-size: 12px;
  color: var(--wn-muted);
  font-weight: 600;
}

.card-stage {
  border: 1px dashed var(--wn-border);
  border-radius: 14px;
  background: var(--wn-surface-soft);
  padding: 12px;
  min-height: 186px;
}

.flash-card {
  perspective: 1000px;
  min-height: 136px;
}

.flash-card-inner {
  position: relative;
  min-height: 136px;
  transition: transform 0.45s ease;
  transform-style: preserve-3d;
}

.flash-card.flipped .flash-card-inner {
  transform: rotateY(180deg);
}

.flash-face {
  position: absolute;
  inset: 0;
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  background: var(--wn-surface);
  padding: 11px;
  display: grid;
  align-content: center;
  gap: 6px;
  backface-visibility: hidden;
}

.flash-back {
  transform: rotateY(180deg);
  background: linear-gradient(180deg, var(--wn-surface), var(--wn-primary-soft));
}

.face-label {
  font-size: 11px;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--wn-muted);
}

.card-meta {
  display: block;
  color: var(--wn-muted);
  font-size: 11px;
}

.flash-face h3,
.prompt {
  font-size: 16px;
  color: var(--wn-ink);
  font-family: 'Fraunces', 'Georgia', serif;
}

.answer-input {
  width: 100%;
  margin-top: 8px;
  border: 1px solid var(--wn-border);
  border-radius: 10px;
  padding: 9px 10px;
  font-size: 13px;
  outline: none;
  color: var(--wn-ink);
  background: var(--wn-surface);
}

.answer-input:focus {
  border-color: var(--wn-primary);
  box-shadow: 0 0 0 3px rgba(31, 78, 216, 0.2);
}

.actions {
  display: flex;
  gap: 7px;
  flex-wrap: wrap;
  margin-top: 10px;
}

.actions button:not(.soft):not(.ghost) {
  border-color: var(--wn-primary);
  background: var(--wn-primary);
  color: var(--wn-on-primary);
}

.option-grid {
  margin-top: 10px;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 7px;
}

.option.selected {
  border-color: var(--wn-primary);
  background: var(--wn-primary-soft);
}

.option.correct {
  border-color: #0f766e;
  background: #e6fffa;
}

.option.wrong {
  border-color: #b45309;
  background: #fff7ed;
}

.review-footer {
  border-top: 1px dashed var(--wn-border);
  padding-top: 8px;
}

.difficulty-actions {
  margin-top: 8px;
  display: flex;
  gap: 7px;
}

.difficulty {
  flex: 1;
}

.difficulty.hard {
  border-color: #f1c5c9;
  background: #fff5f6;
  color: #912018;
}

.difficulty.medium {
  border-color: #efd7a9;
  background: #fff8ea;
  color: #8c5a00;
}

.difficulty.easy {
  border-color: #a7dfb4;
  background: #effff2;
  color: #166534;
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

.empty-state {
  border: 1px dashed var(--wn-border);
  border-radius: 12px;
  padding: 14px;
  text-align: center;
  background: var(--wn-surface);
}

.empty-title {
  font-weight: 600;
  color: var(--wn-ink);
  font-family: 'Fraunces', 'Georgia', serif;
}

.ok {
  margin-top: 8px;
  color: #166534;
  font-size: 12px;
  font-weight: 600;
}

.warn {
  margin-top: 8px;
  color: #9a3412;
  font-size: 12px;
  font-weight: 600;
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
