import type { RouteRecordRaw } from 'vue-router'

const routesAS: Array<RouteRecordRaw> = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/AS/LoginPage.vue'),
  },
  {
    path: '/register',
    redirect: { name: 'login' },
  },
  {
    path: '/logout',
    redirect: { name: 'login' },
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: { name: 'landing' },
  },
]

export default routesAS
