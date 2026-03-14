import type { RouteRecordRaw } from 'vue-router'
const routesDMS: Array<RouteRecordRaw> = [
  {
    path: '/dms',
    name: 'DMSlayout',
    component: () => import('@/pages/DMS/MainLayoutDMS.vue'),
    children: [
      {
        path: '',
        name: 'homeDMS',
        component: () => import('@/pages/DMS/HomePageDMS.vue'),
      },
      {
        path: 'dashboard',
        name: 'dashboardDMS',
        component: () => import('@/pages/DMS/DashboardDMS.vue'),
      },
      {
        path: 'upload',
        name: 'uploadDMS',
        component: () => import('@/pages/DMS/UploadPageDMS.vue'),
      },
    ],
  },
]

export default routesDMS