import type { RouteRecordRaw } from 'vue-router'

const routesFFP: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'landing',
    component: () => import('@/pages/LandingPage.vue'),
    meta: {
      title: 'Nền tảng Ôn tập Đề thi & Flashcards SRS',
      description:
        'Hệ thống ôn luyện đề thi trắc nghiệm (MLN122, PRM393, JFE301, JIT401) và ghi nhớ từ vựng Flashcard Spaced Repetition đa nền tảng Web, Desktop, Extension.',
      canonical: '/',
      robots: 'index, follow',
      schemaType: 'landing',
    },
  },
  {
    path: '/learn',
    name: 'learnLab',
    component: () => import('@/pages/WordsNote/LearnLabPage.vue'),
    meta: {
      title: 'Learn Lab',
      robots: 'noindex, nofollow',
    },
  },
  {
    path: '/privacy-policy',
    name: 'privacyPolicy',
    component: () => import('@/pages/PrivacyPolicyPage.vue'),
    meta: {
      title: 'Chính sách Quyền riêng tư',
      description: 'Chính sách bảo mật thông tin và quyền riêng tư của nền tảng WordsNote.',
      canonical: '/privacy-policy',
      robots: 'index, follow',
    },
  },
  {
    path: '/download',
    name: 'downloadApp',
    component: () => import('@/pages/AppDownloadPage.vue'),
    meta: {
      title: 'Tải Ứng dụng Desktop App & Browser Extension',
      description:
        'Cài đặt WordsNote Desktop cho Windows (MSIX trên Microsoft Store) hoặc tiện ích mở rộng Edge & Chrome.',
      canonical: '/download',
      robots: 'index, follow',
      schemaType: 'download',
    },
  },
  {
    path: '/manage',
    name: 'manageCollections',
    component: () => import('@/pages/WordsNote/StudyHubPage.vue'),
    meta: {
      title: 'Quản lý Bộ thẻ Flashcard',
      robots: 'noindex, nofollow',
    },
  },
  {
    path: '/manage/:deckId/session',
    name: 'manageSession',
    component: () => import('@/pages/WordsNote/StudySessionPage.vue'),
    meta: {
      title: 'Phiên học Flashcard SRS',
      requiresAuth: true,
      robots: 'noindex, nofollow',
    },
  },
  {
    path: '/quiz',
    name: 'quizWorkspace',
    component: () => import('@/views/Quiz/QuizWorkspacePage.vue'),
    meta: {
      title: 'Khám phá Ngân hàng Đề thi Trắc nghiệm',
      description:
        'Ôn luyện trắc nghiệm các môn đại học MLN122, PRM393, JFE301, JIT401 kèm đáp án và giải thích chi tiết.',
      canonical: '/quiz',
      robots: 'index, follow',
      schemaType: 'quizCatalog',
    },
  },
  {
    path: '/quiz/:id',
    name: 'quizSubject',
    component: () => import('@/views/Quiz/QuizWorkspacePage.vue'),
    meta: {
      robots: 'index, follow',
      schemaType: 'quizSubject',
    },
  },
  {
    path: '/quiz/:id/practice',
    name: 'quizPractice',
    component: () => import('@/views/Quiz/QuizWorkspacePage.vue'),
    meta: {
      title: 'Luyện đề Trắc nghiệm',
      robots: 'noindex, nofollow',
    },
  },
]

export default routesFFP
