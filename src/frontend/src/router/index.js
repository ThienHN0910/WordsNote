import { createRouter, createWebHistory } from 'vue-router'
import authService from '../services/authService'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('../modules/auth/LoginView.vue'),
  },
  {
    path: '/',
    name: 'Dashboard',
    component: () => import('../modules/decks/DashboardView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/decks/:id',
    name: 'DeckDetail',
    component: () => import('../modules/decks/DeckDetailView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/decks/:id/play',
    name: 'FlashcardPlayer',
    component: () => import('../modules/flashcards/FlashcardPlayerView.vue'),
    meta: { requiresAuth: true },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to, from, next) => {
  if (to.meta.requiresAuth && !authService.isAuthenticated()) {
    next('/login')
  } else if (to.path === '/login' && authService.isAuthenticated()) {
    next('/')
  } else {
    next()
  }
})

export default router
