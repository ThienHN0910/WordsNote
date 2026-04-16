const LOCAL_CARDS_KEY = 'wordsnote_local_cards';

function getLocalCards() {
  return new Promise((resolve) => {
    chrome.storage.local.get([LOCAL_CARDS_KEY], (result) => {
      const cards = Array.isArray(result[LOCAL_CARDS_KEY]) ? result[LOCAL_CARDS_KEY] : [];
      resolve(cards);
    });
  });
}

function saveLocalCards(cards) {
  return new Promise((resolve) => {
    chrome.storage.local.set({ [LOCAL_CARDS_KEY]: cards }, () => resolve());
  });
}

chrome.runtime.onMessage.addListener(function(message, sender, sendResponse) {
  if (message.action !== 'saveToInbox') {
    return false;
  }

  (async () => {
    try {
      const front = String(message.text || '').trim();
      if (!front) {
        sendResponse({ success: false, error: 'No text to save' });
        return;
      }

      const cards = await getLocalCards();
      const now = new Date().toISOString();

      cards.unshift({
        id: `local-${Math.random().toString(36).slice(2, 10)}-${Date.now()}`,
        front,
        back: '',
        hint: 'Saved from web',
        createdAt: now,
        dueAt: now,
        streak: 0,
      });

      await saveLocalCards(cards);
      sendResponse({ success: true });
    } catch (err) {
      sendResponse({ success: false, error: err?.message || 'Unknown error' });
    }
  })();

  return true;
});
