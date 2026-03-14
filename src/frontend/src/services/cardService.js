import api from './api'

export default {
  getCards: (deckId) => api.get('/api/cards', { params: { deckId } }),
  createCard: (data) => api.post('/api/cards', data),
  getDueCards: () => api.get('/api/cards/due'),
  reviewCard: (id, result) => api.post(`/api/cards/${id}/review`, { result }),
  deleteCard: (id) => api.delete(`/api/cards/${id}`),
}
