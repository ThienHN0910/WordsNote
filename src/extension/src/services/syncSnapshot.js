function normalizeSyncText(value) {
  return String(value || '').trim().toLowerCase();
}

function normalizeTags(tags) {
  return [...(Array.isArray(tags) ? tags : [])]
    .map((tag) => normalizeSyncText(tag))
    .filter(Boolean)
    .sort()
    .join('|');
}

export function createDeckMatchKey(title) {
  return normalizeSyncText(title);
}

export function createDeckFingerprint(deck) {
  return [createDeckMatchKey(deck.title), normalizeSyncText(deck.description)].join('::');
}

export function createCardMatchKey(collectionId, front, back) {
  return [
    normalizeSyncText(collectionId),
    normalizeSyncText(front),
    normalizeSyncText(back),
  ].join('::');
}

export function createCardFingerprint(card) {
  return [
    createCardMatchKey(card.collectionId, card.front, card.back),
    normalizeSyncText(card.hint || ''),
    normalizeTags(card.tags),
  ].join('::');
}
