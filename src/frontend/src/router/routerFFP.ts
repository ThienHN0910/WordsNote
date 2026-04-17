import type { RouteRecordRaw } from 'vue-router'

const routesFFP: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'landing',
    component: () => import('@/pages/LandingPage.vue'),
  },
  {
    path: '/learn',
    name: 'learnLab',
    component: () => import('@/pages/WordsNote/LearnLabPage.vue'),
  },
  {
    path: '/privacy-policy',
    name: 'privacyPolicy',
    component: () => import('@/pages/PrivacyPolicyPage.vue'),
  },
  {
    path: '/manage',
    name: 'manageCollections',
    component: () => import('@/pages/WordsNote/StudyHubPage.vue'),
    meta: {
      requiresAuth: true,
    },
  },
  {
    path: '/manage/:deckId/session',
    name: 'manageSession',
    component: () => import('@/pages/WordsNote/StudySessionPage.vue'),
    meta: {
      requiresAuth: true,
    },
  },
]

export default routesFFP
