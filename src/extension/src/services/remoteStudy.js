import {
  createCardFingerprint,
  createCardMatchKey,
  createDeckFingerprint,
  createDeckMatchKey,
} from './syncSnapshot.js';

const CLOUD_API_BASE_KEY = 'wordsnote_cloud_api_base_url';
const CLOUD_AUTH_TOKEN_KEY = 'wordsnote_cloud_auth_token';
const CLOUD_SYNC_STATE_KEY = 'wordsnote_cloud_sync_state_v1';
const DEFAULT_CLOUD_API_BASE_URL = 'http://words-note.runasp.net';

function normalizeBaseUrl(value) {
  const normalized = String(value || '').trim().replace(/\/+$/, '');
  return normalized || DEFAULT_CLOUD_API_BASE_URL;
}

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
      resolve(raw);
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

async function fetchJson(path, options = {}) {
  const method = String(options.method || 'GET').toUpperCase();
  const baseUrl = normalizeBaseUrl(options.baseUrl);
  const token = String(options.token || '').trim();
  const headers = {
    Accept: 'application/json',
    ...(options.body ? { 'Content-Type': 'application/json' } : {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };

  const response = await fetch(`${baseUrl}${path}`, {
    method,
    headers,
    body: options.body ? JSON.stringify(options.body) : undefined,
  });

  if (!response.ok) {
    const detail = await response.text();
    const compactDetail = detail.replace(/\s+/g, ' ').trim();
    throw new Error(
      compactDetail
        ? `Request failed (${response.status}) for ${path}. ${compactDetail}`
        : `Request failed (${response.status}) for ${path}`,
    );
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

function resolveCollectionId(card) {
  return String(card.collectionId || card.deckId || card.deskId || '').trim();
}

function normalizeSyncState(raw) {
  const state = raw && typeof raw === 'object' ? raw : {};
  const deckIdMap = state.deckIdMap && typeof state.deckIdMap === 'object' ? state.deckIdMap : {};
  const cardIdMap = state.cardIdMap && typeof state.cardIdMap === 'object' ? state.cardIdMap : {};

  return {
    deckIdMap: Object.fromEntries(
      Object.entries(deckIdMap)
        .map(([localId, cloudId]) => [String(localId).trim(), String(cloudId).trim()])
        .filter(([localId, cloudId]) => localId && cloudId),
    ),
    cardIdMap: Object.fromEntries(
      Object.entries(cardIdMap)
        .map(([localId, cloudId]) => [String(localId).trim(), String(cloudId).trim()])
        .filter(([localId, cloudId]) => localId && cloudId),
    ),
  };
}

function normalizeLocalCollection(collection) {
  return {
    id: String(collection.id || collection.Id || '').trim(),
    title: String(collection.title || collection.Title || '').trim(),
    description: String(collection.description || collection.Description || '').trim(),
  };
}

function normalizeLocalCard(card) {
  return {
    id: String(card.id || card.Id || '').trim(),
    collectionId: String(card.collectionId || card.CollectionId || card.deckId || card.DeckId || '').trim(),
    front: String(card.front || card.Front || '').trim(),
    back: String(card.back || card.Back || '').trim(),
    hint: String(card.hint || card.Hint || '').trim(),
    tags: Array.isArray(card.tags || card.Tags)
      ? (card.tags || card.Tags || []).map((tag) => String(tag).trim()).filter(Boolean)
      : [],
  };
}

function normalizeCloudCollection(collection) {
  return {
    id: String(collection.id || collection.Id || '').trim(),
    title: String(collection.title || collection.Title || '').trim(),
    description: String(collection.description || collection.Description || '').trim(),
  };
}

function normalizeCloudCard(card) {
  return {
    id: String(card.id || card.Id || '').trim(),
    collectionId: String(card.collectionId || card.CollectionId || card.deckId || card.DeckId || '').trim(),
    front: String(card.front || card.Front || '').trim(),
    back: String(card.back || card.Back || '').trim(),
    hint: String(card.hint || card.Hint || '').trim(),
    tags: Array.isArray(card.tags || card.Tags)
      ? (card.tags || card.Tags || []).map((tag) => String(tag).trim()).filter(Boolean)
      : [],
  };
}

export async function getCloudApiBaseUrl() {
  const stored = await readStorage(CLOUD_API_BASE_KEY, DEFAULT_CLOUD_API_BASE_URL);
  return normalizeBaseUrl(stored);
}

export async function setCloudApiBaseUrl(value) {
  const normalized = normalizeBaseUrl(value);
  await writeStorage(CLOUD_API_BASE_KEY, normalized);
  return normalized;
}

export async function getCloudAuthToken() {
  const token = await readStorage(CLOUD_AUTH_TOKEN_KEY, '');
  return String(token || '').trim();
}

export async function setCloudAuthToken(value) {
  const token = String(value || '').trim();
  await writeStorage(CLOUD_AUTH_TOKEN_KEY, token);
  return token;
}

export async function clearCloudAuthToken() {
  await writeStorage(CLOUD_AUTH_TOKEN_KEY, '');
}

export async function fetchPublicLearnSnapshot(baseUrl) {
  const resolvedBaseUrl = normalizeBaseUrl(baseUrl);

  const [collections, cards] = await Promise.all([
    fetchJson('/api/collections', { baseUrl: resolvedBaseUrl }),
    fetchJson('/api/cards', { baseUrl: resolvedBaseUrl }),
  ]);

  const collectionMap = new Map(
    (Array.isArray(collections) ? collections : []).map((collection) => [collection.id, collection]),
  );

  const normalizedCards = (Array.isArray(cards) ? cards : [])
    .filter((card) => String(card.front || '').trim().length > 0)
    .map((card) => {
      const collectionId = resolveCollectionId(card);
      const collection = collectionMap.get(collectionId);

      return {
        id: String(card.id || ''),
        front: String(card.front || '').trim(),
        back: String(card.back || '').trim(),
        hint: card.hint ? String(card.hint) : '',
        dueAt: card.dueAt || null,
        streak: Number(card.streak || 0),
        collectionId,
        collectionTitle: collection ? String(collection.title || '') : '',
        source: 'cloud',
      };
    });

  return {
    baseUrl: resolvedBaseUrl,
    collections: collectionMap.size,
    cards: normalizedCards,
  };
}

export async function syncLocalSnapshotToCloud(payload = {}) {
  const baseUrl = normalizeBaseUrl(payload.baseUrl || (await getCloudApiBaseUrl()));
  const token = String(payload.token || (await getCloudAuthToken())).trim();

  if (!token) {
    throw new Error('Cloud auth token is required to sync local data to cloud.');
  }

  const localCollections = (Array.isArray(payload.collections) ? payload.collections : [])
    .map((item) => normalizeLocalCollection(item))
    .filter((item) => item.id && item.title);
  const localCards = (Array.isArray(payload.cards) ? payload.cards : [])
    .map((item) => normalizeLocalCard(item))
    .filter((item) => item.id && item.collectionId && item.front);

  if (localCollections.length === 0) {
    return {
      deckCount: 0,
      uploadedCards: 0,
      updatedCards: 0,
      skippedCards: 0,
    };
  }

  const syncState = normalizeSyncState(await readStorage(CLOUD_SYNC_STATE_KEY, {}));
  const [rawCloudCollections, rawCloudCards] = await Promise.all([
    fetchJson('/api/collections', { baseUrl, token }),
    fetchJson('/api/cards', { baseUrl, token }),
  ]);

  const cloudCollections = (Array.isArray(rawCloudCollections) ? rawCloudCollections : [])
    .map((item) => normalizeCloudCollection(item))
    .filter((item) => item.id);
  const cloudCards = (Array.isArray(rawCloudCards) ? rawCloudCards : [])
    .map((item) => normalizeCloudCard(item))
    .filter((item) => item.id && item.collectionId);

  const cloudCollectionById = new Map(cloudCollections.map((item) => [item.id, item]));
  const cloudCollectionByMatchKey = new Map(cloudCollections.map((item) => [createDeckMatchKey(item.title), item]));

  const nextDeckIdMap = {};
  for (const localCollection of localCollections) {
    const mappedId = syncState.deckIdMap[localCollection.id];
    const mappedCollection = mappedId ? cloudCollectionById.get(mappedId) : null;
    let cloudCollection = mappedCollection || cloudCollectionByMatchKey.get(createDeckMatchKey(localCollection.title));

    if (!cloudCollection) {
      const created = normalizeCloudCollection(
        await fetchJson('/api/collections', {
          method: 'POST',
          baseUrl,
          token,
          body: {
            title: localCollection.title,
            description: localCollection.description,
          },
        }),
      );
      cloudCollection = created;
      cloudCollectionById.set(created.id, created);
      cloudCollectionByMatchKey.set(createDeckMatchKey(created.title), created);
    } else {
      const localFingerprint = createDeckFingerprint(localCollection);
      const cloudFingerprint = createDeckFingerprint(cloudCollection);
      if (localFingerprint !== cloudFingerprint) {
        const updated = normalizeCloudCollection(
          await fetchJson(`/api/collections/${encodeURIComponent(cloudCollection.id)}`, {
            method: 'PUT',
            baseUrl,
            token,
            body: {
              title: localCollection.title,
              description: localCollection.description,
            },
          }),
        );
        cloudCollection = updated;
        cloudCollectionById.set(updated.id, updated);
        cloudCollectionByMatchKey.set(createDeckMatchKey(updated.title), updated);
      }
    }

    nextDeckIdMap[localCollection.id] = cloudCollection.id;
  }

  const cloudCardById = new Map(cloudCards.map((item) => [item.id, item]));
  const cloudCardByMatchKey = new Map(
    cloudCards.map((item) => [createCardMatchKey(item.collectionId, item.front, item.back), item]),
  );

  const nextCardIdMap = {};
  let uploadedCards = 0;
  let updatedCards = 0;
  let skippedCards = 0;

  for (const localCard of localCards) {
    const cloudCollectionId = nextDeckIdMap[localCard.collectionId];
    if (!cloudCollectionId) {
      continue;
    }

    const mappedId = syncState.cardIdMap[localCard.id];
    const mappedCard = mappedId ? cloudCardById.get(mappedId) : null;
    const existingCloudCard = mappedCard || cloudCardByMatchKey.get(createCardMatchKey(cloudCollectionId, localCard.front, localCard.back));

    if (!existingCloudCard) {
      const created = normalizeCloudCard(
        await fetchJson('/api/cards', {
          method: 'POST',
          baseUrl,
          token,
          body: {
            collectionId: cloudCollectionId,
            front: localCard.front,
            back: localCard.back,
            hint: localCard.hint,
            tags: localCard.tags,
          },
        }),
      );
      cloudCardById.set(created.id, created);
      cloudCardByMatchKey.set(createCardMatchKey(created.collectionId, created.front, created.back), created);
      nextCardIdMap[localCard.id] = created.id;
      uploadedCards += 1;
      continue;
    }

    const localFingerprint = createCardFingerprint({
      ...localCard,
      collectionId: cloudCollectionId,
    });
    const cloudFingerprint = createCardFingerprint(existingCloudCard);
    if (localFingerprint === cloudFingerprint) {
      nextCardIdMap[localCard.id] = existingCloudCard.id;
      skippedCards += 1;
      continue;
    }

    const updated = normalizeCloudCard(
      await fetchJson(`/api/cards/${encodeURIComponent(existingCloudCard.id)}`, {
        method: 'PUT',
        baseUrl,
        token,
        body: {
          collectionId: cloudCollectionId,
          front: localCard.front,
          back: localCard.back,
          hint: localCard.hint,
          tags: localCard.tags,
        },
      }),
    );

    cloudCardById.set(updated.id, updated);
    cloudCardByMatchKey.set(createCardMatchKey(updated.collectionId, updated.front, updated.back), updated);
    nextCardIdMap[localCard.id] = updated.id;
    updatedCards += 1;
  }

  await writeStorage(CLOUD_SYNC_STATE_KEY, normalizeSyncState({
    deckIdMap: nextDeckIdMap,
    cardIdMap: nextCardIdMap,
  }));

  return {
    deckCount: localCollections.length,
    uploadedCards,
    updatedCards,
    skippedCards,
  };
}
