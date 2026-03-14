import type { RouteRecordRaw } from 'vue-router'
const routesCMS: Array<RouteRecordRaw> = [
  {
    path: '/cms',
    name: 'CMSlayout',
    component: () => import('@/pages/CMS/MainLayoutCMS.vue'),
    children: [
      {
        path: '',
        name: 'homeCMS',
        component: () => import('@/pages/CMS/HomePageCMS.vue'),
      },
      {
        path: 'create-post',
        name: 'createPostCMS',
        component: () => import('@/pages/CMS/CreatePostCMS.vue'),
      },
      {
        path: 'dashboard',
        name: 'dashboardCMS',
        component: () => import('@/pages/CMS/DashboardCMS.vue'),
      },
    ],
  },
]

export default routesCMS