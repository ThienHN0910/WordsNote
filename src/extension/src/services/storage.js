export function saveToken(token) {
  if (typeof chrome !== 'undefined' && chrome.storage) {
    chrome.storage.local.set({ token });
  } else {
    localStorage.setItem('token', token);
  }
}

export function clearToken() {
  if (typeof chrome !== 'undefined' && chrome.storage) {
    chrome.storage.local.remove(['token']);
  } else {
    localStorage.removeItem('token');
  }
}

export function getToken() {
  return new Promise((resolve) => {
    if (typeof chrome !== 'undefined' && chrome.storage) {
      chrome.storage.local.get(['token'], (result) => resolve(result.token || null));
    } else {
      resolve(localStorage.getItem('token'));
    }
  });
}
