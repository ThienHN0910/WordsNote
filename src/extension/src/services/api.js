const API_URL = 'http://localhost:5000';

function getToken() {
  return new Promise((resolve) => {
    if (typeof chrome !== 'undefined' && chrome.storage) {
      chrome.storage.local.get(['token'], (result) => resolve(result.token || null));
    } else {
      resolve(localStorage.getItem('token'));
    }
  });
}

export async function apiGet(path) {
  const token = await getToken();
  const response = await fetch(`${API_URL}${path}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  if (!response.ok) throw new Error(response.statusText);
  return response.json();
}

export async function apiPost(path, data) {
  const token = await getToken();
  const response = await fetch(`${API_URL}${path}`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });
  if (!response.ok) throw new Error(response.statusText);
  return response.json();
}

export async function login(username, password) {
  const response = await fetch(`${API_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password })
  });
  if (!response.ok) throw new Error('Login failed');
  return response.json();
}
