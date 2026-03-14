import type { RouteRecordRaw } from 'vue-router'

const routesFFP: Array<RouteRecordRaw> = []

const baseChildren: RouteRecordRaw[] = [
  {
    path: '',
    name: 'home',
    component: () => import('@/pages/HomePage.vue'),
  },
  {
    path: 'study',
    name: 'studyHub',
    component: () => import('@/pages/WordsNote/StudyHubPage.vue'),
  },
  {
    path: 'study/:deckId/session',
    name: 'studySession',
    component: () => import('@/pages/WordsNote/StudySessionPage.vue'),
  },
  {
    path: 'dashboard',
    name: 'dashboard',
    component: () => import('@/pages/Dashboard.vue'),
  },
  {
    path: 'about',
    name: 'about',
    component: () => import('@/pages/About.vue'),
  },
]

if (import.meta.env.DEV) {
  baseChildren.push({
    path: 'examples',
    name: 'examples',
    component: () => import('@/pages/Examples.vue'),
  })
}

routesFFP.push({
  path: '/',
  name: 'FFPlayout',
  component: () => import('@/pages/MainLayout.vue'),
  children: baseChildren,
})

export default routesFFP
