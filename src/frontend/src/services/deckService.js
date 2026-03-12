import api from './api'

export default {
  getDecks: () => api.get('/api/decks'),
  getDeck: (id) => api.get(`/api/decks/${id}`),
  createDeck: (data) => api.post('/api/decks', data),
  updateDeck: (id, data) => api.put(`/api/decks/${id}`, data),
  deleteDeck: (id) => api.delete(`/api/decks/${id}`),
  importCsv: (id, file) => {
    const formData = new FormData()
    formData.append('file', file)
    return api.post(`/api/decks/${id}/import`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
  resetDeck: (id) => api.post(`/api/decks/${id}/reset`),
}
