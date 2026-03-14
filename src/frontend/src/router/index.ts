import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import routesAS from './routerAS'
import routesFFP from './routerFFP'

const routes: Array<RouteRecordRaw> = [
  ...routesFFP,
  ...routesAS,
]
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...routes
  ],
})

export default router
