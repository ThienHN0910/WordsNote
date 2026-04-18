const LOCAL_CARDS_KEY = 'wordsnote_local_cards';
const LOCAL_COLLECTIONS_KEY = 'wordsnote_local_collections';
const CLOUD_SYNC_SOURCE = 'cloud-sync';
const DEFAULT_COLLECTION_ID = 'local-inbox';
const DEFAULT_COLLECTION_TITLE = 'Local Inbox';
const DEFAULT_COLLECTION_DESCRIPTION = 'Default collection for quick captures from the browser extension.';

function readStorage(key, fallbackValue) {
  return new Promise((resolve) => {
    if (typeof chrome !== 'undefined' && chrome.storage) {
      chrome.storage.local.get([key], (result) => resolve(result[key] ?? fallbackValue));
      return;
    }

    const raw = localStorage.getItem(key);
    if (raw === null) {
      resolve(fallbackValue);
      return;
    }

    try {
      resolve(JSON.parse(raw));
    } catch {
      resolve(fallbackValue);
    }
  });
}

function writeStorage(key, value) {
  return new Promise((resolve) => {
    if (typeof chrome !== 'undefined' && chrome.storage) {
      chrome.storage.local.set({ [key]: value }, () => resolve());
      return;
    }

    localStorage.setItem(key, JSON.stringify(value));
    resolve();
  });
}

function nowIso() {
  return new Date().toISOString();
}

function createSystemCollection(now = nowIso()) {
  return {
    id: DEFAULT_COLLECTION_ID,
    title: DEFAULT_COLLECTION_TITLE,
    description: DEFAULT_COLLECTION_DESCRIPTION,
    createdAt: now,
    updatedAt: now,
    isSystem: true,
  };
}

function normalizeCollection(collection, now = nowIso()) {
  const id = String(collection?.id || '').trim() || `local-col-${Math.random().toString(36).slice(2, 10)}-${Date.now()}`;
  const title = String(collection?.title || '').trim() || 'Untitled Collection';
  const description = String(collection?.description || '').trim();
  const createdAt = String(collection?.createdAt || now);
  const updatedAt = String(collection?.updatedAt || createdAt || now);
  const isSystem = id === DEFAULT_COLLECTION_ID || Boolean(collection?.isSystem);

  return {
    id,
    title,
    description,
    createdAt,
    updatedAt,
    isSystem,
  };
}

function normalizeCard(card, fallbackCollectionId = DEFAULT_COLLECTION_ID, fallbackCollectionTitle = DEFAULT_COLLECTION_TITLE, now = nowIso()) {
  const front = String(card?.front || '').trim();
  const back = String(card?.back || '').trim();
  const hint = String(card?.hint || '').trim();

  return {
    id: String(card?.id || '').trim() || `local-${Math.random().toString(36).slice(2, 10)}-${Date.now()}`,
    front,
    back,
    hint,
    createdAt: String(card?.createdAt || now),
    dueAt: String(card?.dueAt || card?.createdAt || now),
    streak: Number(card?.streak || 0),
    lastReviewedAt: card?.lastReviewedAt ? String(card.lastReviewedAt) : null,
    source: card?.source ? String(card.source) : undefined,
    cloudCardId: card?.cloudCardId ? String(card.cloudCardId) : undefined,
    syncedAt: card?.syncedAt ? String(card.syncedAt) : undefined,
    collectionId: String(card?.collectionId || fallbackCollectionId).trim() || fallbackCollectionId,
    collectionTitle: String(card?.collectionTitle || fallbackCollectionTitle).trim() || fallbackCollectionTitle,
  };
}

function normalizeSnapshot(rawCollections, rawCards) {
  const now = nowIso();

  const collectionMap = new Map();
  if (Array.isArray(rawCollections)) {
    for (const rawCollection of rawCollections) {
      const normalized = normalizeCollection(rawCollection, now);
      collectionMap.set(normalized.id, normalized);
    }
  }

  if (!collectionMap.has(DEFAULT_COLLECTION_ID)) {
    collectionMap.set(DEFAULT_COLLECTION_ID, createSystemCollection(now));
  }

  const normalizedCards = [];
  if (Array.isArray(rawCards)) {
    for (const rawCard of rawCards) {
      const normalized = normalizeCard(rawCard, DEFAULT_COLLECTION_ID, DEFAULT_COLLECTION_TITLE, now);
      if (!normalized.front) {
        continue;
      }

      const existingCollection = collectionMap.get(normalized.collectionId);
      if (!existingCollection) {
        const fallbackCollection = normalizeCollection(
          {
            id: normalized.collectionId,
            title: normalized.collectionTitle || 'Imported Collection',
            description: '',
          },
          now,
        );
        collectionMap.set(fallbackCollection.id, fallbackCollection);
      }

      const resolvedCollection = collectionMap.get(normalized.collectionId) || collectionMap.get(DEFAULT_COLLECTION_ID);
      normalized.collectionId = resolvedCollection.id;
      normalized.collectionTitle = resolvedCollection.title;
      normalizedCards.push(normalized);
    }
  }

  return {
    collections: [...collectionMap.values()],
    cards: normalizedCards,
  };
}

async function loadSnapshot() {
  const [rawCollections, rawCards] = await Promise.all([
    readStorage(LOCAL_COLLECTIONS_KEY, []),
    readStorage(LOCAL_CARDS_KEY, []),
  ]);

  return normalizeSnapshot(rawCollections, rawCards);
}

async function saveSnapshot(snapshot) {
  await Promise.all([
    writeStorage(LOCAL_COLLECTIONS_KEY, snapshot.collections),
    writeStorage(LOCAL_CARDS_KEY, snapshot.cards),
  ]);
}

export async function getLocalCollections() {
  const snapshot = await loadSnapshot();
  return snapshot.collections;
}

export async function saveLocalCollections(collections) {
  const snapshot = await loadSnapshot();
  const normalized = normalizeSnapshot(collections, snapshot.cards);
  await saveSnapshot(normalized);
  return normalized.collections;
}

export async function createLocalCollection(title, description = '') {
  const normalizedTitle = String(title || '').trim();
  if (!normalizedTitle) {
    return null;
  }

  const snapshot = await loadSnapshot();
  const now = nowIso();
  const collection = {
    id: `local-col-${Math.random().toString(36).slice(2, 10)}-${Date.now()}`,
    title: normalizedTitle,
    description: String(description || '').trim(),
    createdAt: now,
    updatedAt: now,
    isSystem: false,
  };

  const nextCollections = [collection, ...snapshot.collections.filter((item) => item.id !== collection.id)];
  const normalized = normalizeSnapshot(nextCollections, snapshot.cards);
  await saveSnapshot(normalized);
  return normalized.collections.find((item) => item.id === collection.id) || null;
}

export async function updateLocalCollection(collectionId, patch = {}) {
  const targetId = String(collectionId || '').trim();
  if (!targetId) {
    return null;
  }

  const snapshot = await loadSnapshot();
  const collection = snapshot.collections.find((item) => item.id === targetId);
  if (!collection) {
    return null;
  }

  if (collection.isSystem && patch.title) {
    patch.title = collection.title;
  }

  const nextTitle = patch.title !== undefined ? String(patch.title || '').trim() : collection.title;
  const nextDescription = patch.description !== undefined
    ? String(patch.description || '').trim()
    : collection.description;

  if (!nextTitle) {
    return null;
  }

  const updatedCollection = {
    ...collection,
    title: nextTitle,
    description: nextDescription,
    updatedAt: nowIso(),
  };

  const nextCollections = snapshot.collections.map((item) =>
    item.id === targetId ? updatedCollection : item,
  );

  const nextCards = snapshot.cards.map((card) =>
    card.collectionId === targetId
      ? { ...card, collectionTitle: updatedCollection.title }
      : card,
  );

  const normalized = normalizeSnapshot(nextCollections, nextCards);
  await saveSnapshot(normalized);
  return normalized.collections.find((item) => item.id === targetId) || null;
}

export async function deleteLocalCollection(collectionId) {
  const targetId = String(collectionId || '').trim();
  if (!targetId || targetId === DEFAULT_COLLECTION_ID) {
    return false;
  }

  const snapshot = await loadSnapshot();
  const exists = snapshot.collections.some((item) => item.id === targetId);
  if (!exists) {
    return false;
  }

  const nextCollections = snapshot.collections.filter((item) => item.id !== targetId);
  const nextCards = snapshot.cards.filter((card) => card.collectionId !== targetId);
  const normalized = normalizeSnapshot(nextCollections, nextCards);
  await saveSnapshot(normalized);
  return true;
}

export async function getLocalCards() {
  const snapshot = await loadSnapshot();
  return snapshot.cards;
}

export async function saveLocalCards(cards) {
  const snapshot = await loadSnapshot();
  const normalized = normalizeSnapshot(snapshot.collections, cards);
  await saveSnapshot(normalized);
  return normalized.cards;
}

export async function getCardsByCollection(collectionId) {
  const cards = await getLocalCards();
  const targetId = String(collectionId || '').trim();
  if (!targetId) {
    return cards;
  }

  return cards.filter((card) => card.collectionId === targetId);
}

export async function createLocalCard(payload = {}) {
  const snapshot = await loadSnapshot();
  const front = String(payload.front || '').trim();
  if (!front) {
    return null;
  }

  const now = nowIso();
  const requestedCollectionId = String(payload.collectionId || DEFAULT_COLLECTION_ID).trim() || DEFAULT_COLLECTION_ID;
  const collection = snapshot.collections.find((item) => item.id === requestedCollectionId)
    || snapshot.collections.find((item) => item.id === DEFAULT_COLLECTION_ID)
    || createSystemCollection(now);

  const card = {
    id: `local-${Math.random().toString(36).slice(2, 10)}-${Date.now()}`,
    front,
    back: String(payload.back || '').trim(),
    hint: String(payload.hint || '').trim(),
    createdAt: now,
    dueAt: now,
    streak: 0,
    lastReviewedAt: null,
    collectionId: collection.id,
    collectionTitle: collection.title,
  };

  const nextCards = [card, ...snapshot.cards];
  const normalized = normalizeSnapshot(snapshot.collections, nextCards);
  await saveSnapshot(normalized);
  return normalized.cards.find((item) => item.id === card.id) || null;
}

export async function updateLocalCard(cardId, patch = {}) {
  const targetId = String(cardId || '').trim();
  if (!targetId) {
    return null;
  }

  const snapshot = await loadSnapshot();
  const card = snapshot.cards.find((item) => item.id === targetId);
  if (!card) {
    return null;
  }

  const nextFront = patch.front !== undefined ? String(patch.front || '').trim() : card.front;
  if (!nextFront) {
    return null;
  }

  const requestedCollectionId = patch.collectionId !== undefined
    ? String(patch.collectionId || '').trim()
    : card.collectionId;

  const collection = snapshot.collections.find((item) => item.id === requestedCollectionId)
    || snapshot.collections.find((item) => item.id === DEFAULT_COLLECTION_ID)
    || createSystemCollection();

  const updatedCard = {
    ...card,
    front: nextFront,
    back: patch.back !== undefined ? String(patch.back || '').trim() : card.back,
    hint: patch.hint !== undefined ? String(patch.hint || '').trim() : card.hint,
    collectionId: collection.id,
    collectionTitle: collection.title,
  };

  const nextCards = snapshot.cards.map((item) => (item.id === targetId ? updatedCard : item));
  const normalized = normalizeSnapshot(snapshot.collections, nextCards);
  await saveSnapshot(normalized);
  return normalized.cards.find((item) => item.id === targetId) || null;
}

export async function deleteLocalCard(cardId) {
  const targetId = String(cardId || '').trim();
  if (!targetId) {
    return false;
  }

  const snapshot = await loadSnapshot();
  const nextCards = snapshot.cards.filter((card) => card.id !== targetId);
  if (nextCards.length === snapshot.cards.length) {
    return false;
  }

  const normalized = normalizeSnapshot(snapshot.collections, nextCards);
  await saveSnapshot(normalized);
  return true;
}

export async function addLocalCard(front, back = '', hint = 'Saved from web') {
  return createLocalCard({
    front,
    back,
    hint,
    collectionId: DEFAULT_COLLECTION_ID,
  });
}

export async function getDueLocalCards() {
  const cards = await getLocalCards();
  const now = Date.now();
  return cards.filter((card) => {
    const dueAt = new Date(card.dueAt || card.createdAt || 0).getTime();
    return dueAt <= now;
  });
}

function nextIntervalDays(difficulty, streak) {
  const baseDays = difficulty === 'hard' ? 1 : difficulty === 'medium' ? 2 : 4;
  return baseDays + Math.max(streak - 1, 0);
}

export async function reviewLocalCard(cardId, difficulty) {
  const cards = await getLocalCards();
  const index = cards.findIndex((card) => card.id === cardId);
  if (index === -1) {
    return null;
  }

  const card = cards[index];
  const normalizedDifficulty = ['hard', 'medium', 'easy'].includes(difficulty) ? difficulty : 'hard';
  const streak = normalizedDifficulty === 'hard' ? 0 : (card.streak || 0) + 1;
  const now = new Date();
  const dueAt = new Date(now);
  dueAt.setDate(dueAt.getDate() + nextIntervalDays(normalizedDifficulty, streak));

  cards[index] = {
    ...card,
    streak,
    dueAt: dueAt.toISOString(),
    lastReviewedAt: now.toISOString(),
  };

  await saveLocalCards(cards);
  return cards[index];
}

function normalizeCloudCardForLocal(card, nowIso) {
  const originalId = String(card.id || '').trim();
  const cardId = originalId || `missing-${Math.random().toString(36).slice(2, 10)}`;
  const collectionId = String(card.collectionId || 'cloud-default').trim() || 'cloud-default';
  const collectionTitle = String(card.collectionTitle || 'Cloud Collection').trim() || 'Cloud Collection';

  return {
    id: `cloud-${cardId}`,
    front: String(card.front || '').trim(),
    back: String(card.back || '').trim(),
    hint: String(card.hint || '').trim(),
    createdAt: nowIso,
    dueAt: nowIso,
    streak: Number(card.streak || 0),
    lastReviewedAt: null,
    source: CLOUD_SYNC_SOURCE,
    cloudCardId: cardId,
    collectionId,
    collectionTitle,
    syncedAt: nowIso,
  };
}

export async function syncCloudCardsToLocal(cloudCards, options = {}) {
  const replacePreviousSynced = options.replacePreviousSynced !== false;
  const localCards = await getLocalCards();
  const nowIso = new Date().toISOString();

  const incomingCards = Array.isArray(cloudCards) ? cloudCards : [];
  const syncedCards = incomingCards
    .filter((card) => String(card.front || '').trim().length > 0)
    .map((card) => normalizeCloudCardForLocal(card, nowIso));

  const dedupedSyncedCards = [];
  const seenIds = new Set();
  for (const card of syncedCards) {
    if (seenIds.has(card.id)) {
      continue;
    }

    seenIds.add(card.id);
    dedupedSyncedCards.push(card);
  }

  const retainedLocalCards = replacePreviousSynced
    ? localCards.filter((card) => card.source !== CLOUD_SYNC_SOURCE)
    : [...localCards];

  const mergedCards = [...dedupedSyncedCards, ...retainedLocalCards];
  await saveLocalCards(mergedCards);

  return {
    synced: dedupedSyncedCards.length,
    totalLocal: mergedCards.length,
  };
}
