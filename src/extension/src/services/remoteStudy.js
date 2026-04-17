const CLOUD_API_BASE_KEY = 'wordsnote_cloud_api_base_url';
const DEFAULT_CLOUD_API_BASE_URL = 'http://localhost:3000';

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

async function fetchPublicJson(path, baseUrl) {
  const url = `${normalizeBaseUrl(baseUrl)}${path}`;
  const response = await fetch(url, {
    method: 'GET',
    headers: {
      Accept: 'application/json',
    },
  });

  if (!response.ok) {
    throw new Error(`Request failed (${response.status}) for ${path}`);
  }

  return response.json();
}

function resolveCollectionId(card) {
  return card.collectionId || card.deckId || card.deskId || '';
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

export async function fetchPublicLearnSnapshot(baseUrl) {
  const resolvedBaseUrl = normalizeBaseUrl(baseUrl);

  const [collections, cards] = await Promise.all([
    fetchPublicJson('/api/collections', resolvedBaseUrl),
    fetchPublicJson('/api/cards', resolvedBaseUrl),
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
