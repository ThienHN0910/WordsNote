import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import routesDMS from './routerDMS'
import routesAS from './routerAS'
import routesFFP from './routerFFP'
import routesCMS from './routerCMS'

const routes: Array<RouteRecordRaw> = [
  ...routesFFP,
  ...routesAS,
  ...routesDMS,
  ...routesCMS,
]
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...routes
  ],
})

export default router
