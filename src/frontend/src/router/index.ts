import { createRouter, createWebHistory } from 'vue-router'
import routesAS from './routerAS'
import routesFFP from './routerFFP'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { pinia } from '@/stores/pinia'
import {
  updateSeo,
  getLandingJsonLd,
  getQuizCatalogJsonLd,
  getQuizSubjectJsonLd,
  getDownloadJsonLd,
  type SeoOptions,
} from '@/utils/seo'

const SUBJECT_SEO_MAP: Record<
  string,
  { id: string; code: string; title: string; totalQuestions: number; description: string }
> = {
  mln122: {
    id: 'mln122',
    code: 'MLN122',
    title: 'Triết học Mác - Lênin',
    totalQuestions: 1180,
    description:
      'Ngân hàng 1.180 câu hỏi trắc nghiệm ôn thi môn Triết học Mác - Lênin (MLN122) kèm đáp án chính xác và lời giải thích chi tiết.',
  },
  prm393: {
    id: 'prm393',
    code: 'PRM393',
    title: 'Lập trình Di động Android',
    totalQuestions: 530,
    description:
      'Đề thi trắc nghiệm Lập trình Di động Android (PRM393) tổng hợp đầy đủ câu hỏi thực hành, vòng đời Activity và Kotlin/Java.',
  },
  jfe301: {
    id: 'jfe301',
    code: 'JFE301',
    title: 'Frontend Frameworks (React & Vue)',
    totalQuestions: 320,
    description:
      'Bộ câu hỏi trắc nghiệm Frontend Frameworks (JFE301) ôn tập kiến thức React hooks, Vue 3 reactivity và state management.',
  },
  jit401: {
    id: 'jit401',
    code: 'JIT401',
    title: 'Quản trị Hạ tầng IT',
    totalQuestions: 224,
    description:
      'Ngân hàng đề thi trắc nghiệm Quản trị Hạ tầng IT (JIT401) bao quát các chủ đề máy chủ, ảo hóa, mạng và bảo mật.',
  },
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [...routesFFP, ...routesAS],
})

router.beforeEach((to) => {
  if (!to.matched.some((record) => record.meta.requiresAuth)) {
    return true
  }

  const authStore = useAuthStore(pinia)
  authStore.rehydrateFromPersistedState()

  if (authStore.hasAuthSession) {
    return true
  }

  return {
    name: 'login',
    query: {
      redirect: to.fullPath,
    },
  }
})

router.afterEach((to) => {
  const meta = to.meta || {}
  const seoConfig: SeoOptions = {
    title: meta.title as string | undefined,
    description: meta.description as string | undefined,
    canonical: (meta.canonical as string) || to.path,
    robots: (meta.robots as SeoOptions['robots']) || 'index, follow',
  }

  // Dynamic SEO for Quiz Subject Detail
  if (to.name === 'quizSubject') {
    const rawId = (to.params.id as string)?.toLowerCase()
    const subject = rawId ? SUBJECT_SEO_MAP[rawId] : null
    if (subject) {
      seoConfig.title = `Ôn thi ${subject.code} · ${subject.title}`
      seoConfig.description = subject.description
      seoConfig.canonical = `/quiz/${subject.id}`
      seoConfig.jsonLd = getQuizSubjectJsonLd(subject)
    } else {
      const codeUpper = (rawId || 'MÔN HỌC').toUpperCase()
      seoConfig.title = `Ôn thi Trắc nghiệm ${codeUpper}`
      seoConfig.canonical = `/quiz/${rawId}`
    }
  } else if (meta.schemaType === 'landing') {
    seoConfig.jsonLd = getLandingJsonLd()
  } else if (meta.schemaType === 'quizCatalog') {
    seoConfig.jsonLd = getQuizCatalogJsonLd()
  } else if (meta.schemaType === 'download') {
    seoConfig.jsonLd = getDownloadJsonLd()
  }

  updateSeo(seoConfig)
})

export default router
