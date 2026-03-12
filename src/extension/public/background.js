const API_URL = 'http://localhost:5000';

chrome.runtime.onMessage.addListener(function(message, sender, sendResponse) {
  if (message.action === 'saveToInbox') {
    chrome.storage.local.get(['token'], async function(result) {
      const token = result.token;
      if (!token) {
        sendResponse({ success: false, error: 'Not authenticated' });
        return;
      }

      try {
        // Find or create Inbox deck
        const decksResp = await fetch(`${API_URL}/api/decks`, {
          headers: { 'Authorization': 'Bearer ' + token }
        });

        let inboxDeckId = null;

        if (decksResp.ok) {
          const decks = await decksResp.json();
          const inboxDeck = decks.find(function(d) {
            return d.name === 'Inbox';
          });
          if (inboxDeck) {
            inboxDeckId = inboxDeck.id;
          }
        }

        if (!inboxDeckId) {
          const createResp = await fetch(`${API_URL}/api/decks`, {
            method: 'POST',
            headers: {
              'Authorization': 'Bearer ' + token,
              'Content-Type': 'application/json'
            },
            body: JSON.stringify({ name: 'Inbox', description: 'Words saved from the web' })
          });
          if (createResp.ok) {
            const newDeck = await createResp.json();
            inboxDeckId = newDeck.id;
          }
        }

        const cardResp = await fetch(`${API_URL}/api/cards`, {
          method: 'POST',
          headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            front: message.text,
            back: '',
            notes: 'Saved from web',
            deckId: inboxDeckId
          })
        });

        if (cardResp.ok) {
          sendResponse({ success: true });
        } else {
          sendResponse({ success: false, error: 'API error' });
        }
      } catch (err) {
        sendResponse({ success: false, error: err.message });
      }
    });

    return true; // Keep message channel open for async response
  }
});
