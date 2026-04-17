const LOCAL_CARDS_KEY = 'wordsnote_local_cards';
const CLOUD_SYNC_SOURCE = 'cloud-sync';

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

export async function getLocalCards() {
  const cards = await readStorage(LOCAL_CARDS_KEY, []);
  return Array.isArray(cards) ? cards : [];
}

export async function saveLocalCards(cards) {
  await writeStorage(LOCAL_CARDS_KEY, cards);
}

export async function addLocalCard(front, back = '', hint = 'Saved from web') {
  const cards = await getLocalCards();
  const now = new Date();

  const card = {
    id: `local-${Math.random().toString(36).slice(2, 10)}-${Date.now()}`,
    front: String(front || '').trim(),
    back: String(back || '').trim(),
    hint,
    createdAt: now.toISOString(),
    dueAt: now.toISOString(),
    streak: 0,
  };

  if (!card.front) {
    return null;
  }

  cards.unshift(card);
  await saveLocalCards(cards);
  return card;
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
