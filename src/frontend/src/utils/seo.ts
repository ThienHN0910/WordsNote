/**
 * SEO & Metadata Utility for WordsNote SPA
 * Provides reactive title, meta tags, canonical URL, OpenGraph, Twitter Cards,
 * and Schema.org JSON-LD structured data management.
 */

export interface SeoOptions {
  title?: string
  description?: string
  canonical?: string
  robots?: 'index, follow' | 'noindex, nofollow' | 'noindex, follow'
  ogType?: 'website' | 'article'
  ogImage?: string
  keywords?: string[]
  jsonLd?: Record<string, unknown> | Array<Record<string, unknown>> | null
}

export const SITE_DOMAIN = 'https://words-note.thienhn.io.vn'
export const SITE_NAME = 'WordsNote'
export const DEFAULT_TITLE = 'WordsNote · Nền tảng Ôn tập Đề thi & Flashcards SRS'
export const DEFAULT_DESCRIPTION =
  'Hệ thống ôn luyện đề thi trắc nghiệm (MLN122, PRM393, JFE301, JIT401) và ghi nhớ từ vựng Flashcard Spaced Repetition đa nền tảng Web, Desktop, Extension.'
export const DEFAULT_OG_IMAGE = `${SITE_DOMAIN}/images/og-preview.png`
export const DEFAULT_KEYWORDS = [
  'WordsNote',
  'ôn thi trắc nghiệm',
  'đề thi trắc nghiệm',
  'MLN122',
  'PRM393',
  'JFE301',
  'JIT401',
  'flashcards',
  'spaced repetition',
  'lặp lại ngắt quãng',
  'ôn thi FPT',
  'học từ vựng',
]

/**
 * Update or create a <meta> element by name or property attribute.
 */
function setMetaTag(attributeName: 'name' | 'property', key: string, content: string) {
  if (typeof document === 'undefined') return
  let el = document.querySelector(`meta[${attributeName}="${key}"]`) as HTMLMetaElement | null
  if (!el) {
    el = document.createElement('meta')
    el.setAttribute(attributeName, key)
    document.head.appendChild(el)
  }
  el.setAttribute('content', content)
}

/**
 * Update or create the canonical <link> element.
 */
function setCanonical(url: string) {
  if (typeof document === 'undefined') return
  let link = document.querySelector('link[rel="canonical"]') as HTMLLinkElement | null
  if (!link) {
    link = document.createElement('link')
    link.setAttribute('rel', 'canonical')
    document.head.appendChild(link)
  }
  link.setAttribute('href', url)
}

/**
 * Inject or update the Schema.org JSON-LD structured data script.
 */
function setJsonLd(data: Record<string, unknown> | Array<Record<string, unknown>> | null) {
  if (typeof document === 'undefined') return
  const SCRIPT_ID = 'wordsnote-schema-jsonld'
  let script = document.getElementById(SCRIPT_ID) as HTMLScriptElement | null

  if (!data) {
    if (script) script.remove()
    return
  }

  if (!script) {
    script = document.createElement('script')
    script.id = SCRIPT_ID
    script.type = 'application/ld+json'
    document.head.appendChild(script)
  }

  script.textContent = JSON.stringify(data)
}

/**
 * Update all SEO metadata for the current view.
 */
export function updateSeo(options: SeoOptions = {}) {
  if (typeof document === 'undefined') return

  const title = options.title ? `${options.title} · ${SITE_NAME}` : DEFAULT_TITLE
  const description = options.description || DEFAULT_DESCRIPTION
  const canonicalUrl = options.canonical
    ? options.canonical.startsWith('http')
      ? options.canonical
      : `${SITE_DOMAIN}${options.canonical}`
    : `${SITE_DOMAIN}/`
  const robots = options.robots || 'index, follow'
  const ogType = options.ogType || 'website'
  const ogImage = options.ogImage || DEFAULT_OG_IMAGE
  const keywords = options.keywords?.length
    ? options.keywords.join(', ')
    : DEFAULT_KEYWORDS.join(', ')

  // 1. Title
  document.title = title

  // 2. Standard Meta
  setMetaTag('name', 'description', description)
  setMetaTag('name', 'keywords', keywords)
  setMetaTag('name', 'robots', robots)
  setCanonical(canonicalUrl)

  // 3. OpenGraph Tags (Facebook, Zalo, LinkedIn)
  setMetaTag('property', 'og:site_name', SITE_NAME)
  setMetaTag('property', 'og:title', title)
  setMetaTag('property', 'og:description', description)
  setMetaTag('property', 'og:type', ogType)
  setMetaTag('property', 'og:url', canonicalUrl)
  setMetaTag('property', 'og:image', ogImage)
  setMetaTag('property', 'og:image:width', '1200')
  setMetaTag('property', 'og:image:height', '630')
  setMetaTag('property', 'og:image:alt', 'WordsNote - Nền tảng Ôn tập Đề thi & Flashcards SRS')
  setMetaTag('property', 'og:locale', 'vi_VN')

  // 4. Twitter Cards
  setMetaTag('name', 'twitter:card', 'summary_large_image')
  setMetaTag('name', 'twitter:title', title)
  setMetaTag('name', 'twitter:description', description)
  setMetaTag('name', 'twitter:image', ogImage)

  // 5. Schema.org JSON-LD
  if (options.jsonLd !== undefined) {
    setJsonLd(options.jsonLd)
  }
}

// ---------------------------------------------------------
// Pre-configured JSON-LD Structured Data Generators
// ---------------------------------------------------------

export function getLandingJsonLd(): Record<string, unknown> {
  return {
    '@context': 'https://schema.org',
    '@graph': [
      {
        '@type': 'WebApplication',
        '@id': `${SITE_DOMAIN}/#webapp`,
        name: 'WordsNote',
        url: SITE_DOMAIN,
        applicationCategory: 'EducationalApplication',
        operatingSystem: 'All',
        browserRequirements: 'Requires JavaScript. Requires HTML5.',
        description: DEFAULT_DESCRIPTION,
        offers: {
          '@type': 'Offer',
          price: '0',
          priceCurrency: 'VND',
        },
        author: {
          '@type': 'Person',
          name: 'ThienHN',
          url: 'https://thienhn.io.vn',
        },
      },
      {
        '@type': 'EducationalOrganization',
        '@id': `${SITE_DOMAIN}/#organization`,
        name: 'WordsNote Learning Ecosystem',
        url: SITE_DOMAIN,
        logo: `${SITE_DOMAIN}/images/og-preview.png`,
        sameAs: ['https://github.com/ThienHN0910/WordsNote'],
      },
      {
        '@type': 'BreadcrumbList',
        itemListElement: [
          {
            '@type': 'ListItem',
            position: 1,
            name: 'Trang chủ',
            item: SITE_DOMAIN,
          },
        ],
      },
    ],
  }
}

export function getQuizCatalogJsonLd(): Record<string, unknown> {
  return {
    '@context': 'https://schema.org',
    '@type': 'ItemList',
    name: 'Ngân hàng Đề thi Trắc nghiệm WordsNote',
    description:
      'Danh sách các bộ câu hỏi ôn thi trắc nghiệm đại học kèm đáp án và giải thích chi tiết',
    url: `${SITE_DOMAIN}/quiz`,
    itemListElement: [
      {
        '@type': 'ListItem',
        position: 1,
        name: 'MLN122 · Triết học Mác - Lênin (1.180 câu hỏi)',
        url: `${SITE_DOMAIN}/quiz/mln122`,
      },
      {
        '@type': 'ListItem',
        position: 2,
        name: 'PRM393 · Lập trình Di động Android (530 câu hỏi)',
        url: `${SITE_DOMAIN}/quiz/prm393`,
      },
      {
        '@type': 'ListItem',
        position: 3,
        name: 'JFE301 · Frontend Frameworks React & Vue (320 câu hỏi)',
        url: `${SITE_DOMAIN}/quiz/jfe301`,
      },
      {
        '@type': 'ListItem',
        position: 4,
        name: 'JIT401 · Quản trị Hạ tầng IT (224 câu hỏi)',
        url: `${SITE_DOMAIN}/quiz/jit401`,
      },
    ],
  }
}

export function getQuizSubjectJsonLd(subject: {
  id: string
  code: string
  title: string
  totalQuestions?: number
  description?: string
}): Record<string, unknown> {
  const url = `${SITE_DOMAIN}/quiz/${subject.id.toLowerCase()}`
  return {
    '@context': 'https://schema.org',
    '@graph': [
      {
        '@type': 'Course',
        '@id': `${url}#course`,
        name: `${subject.code.toUpperCase()} · ${subject.title}`,
        courseCode: subject.code.toUpperCase(),
        description:
          subject.description ||
          `Ngân hàng đề thi trắc nghiệm môn ${subject.code.toUpperCase()} gồm ${subject.totalQuestions || 0} câu hỏi có đáp án và giải thích chi tiết.`,
        provider: {
          '@type': 'Organization',
          name: 'WordsNote',
          sameAs: SITE_DOMAIN,
        },
      },
      {
        '@type': 'Quiz',
        '@id': `${url}#quiz`,
        name: `Đề thi Trắc nghiệm ${subject.code.toUpperCase()}`,
        learningResourceType: 'Practice Quiz',
        educationalLevel: 'Higher Education',
        isAccessibleForFree: true,
        hasPart: {
          '@type': 'Question',
          eduQuestionType: 'Multiple choice',
        },
      },
      {
        '@type': 'BreadcrumbList',
        itemListElement: [
          {
            '@type': 'ListItem',
            position: 1,
            name: 'Trang chủ',
            item: SITE_DOMAIN,
          },
          {
            '@type': 'ListItem',
            position: 2,
            name: 'Khám phá Đề thi',
            item: `${SITE_DOMAIN}/quiz`,
          },
          {
            '@type': 'ListItem',
            position: 3,
            name: subject.code.toUpperCase(),
            item: url,
          },
        ],
      },
    ],
  }
}

export function getDownloadJsonLd(): Record<string, unknown> {
  return {
    '@context': 'https://schema.org',
    '@graph': [
      {
        '@type': 'SoftwareApplication',
        name: 'WordsNote Desktop App',
        operatingSystem: 'Windows 10, Windows 11',
        applicationCategory: 'EducationalApplication',
        downloadUrl: `${SITE_DOMAIN}/download`,
        offers: {
          '@type': 'Offer',
          price: '0',
          priceCurrency: 'VND',
        },
      },
      {
        '@type': 'SoftwareApplication',
        name: 'WordsNote Browser Extension',
        operatingSystem: 'Chromium, Microsoft Edge, Google Chrome',
        applicationCategory: 'BrowserExtension',
        downloadUrl: `${SITE_DOMAIN}/download`,
        offers: {
          '@type': 'Offer',
          price: '0',
          priceCurrency: 'VND',
        },
      },
    ],
  }
}
