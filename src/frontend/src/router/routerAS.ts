import type { RouteRecordRaw } from 'vue-router'

const routesAS: Array<RouteRecordRaw> = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/AS/LoginPage.vue'),
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('@/pages/AS/RegisterPage.vue'),
  },
]

export default routesAS
