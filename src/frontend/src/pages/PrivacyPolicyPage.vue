<template>
  <section class="privacy-wrap">
    <header class="privacy-hero">
      <p class="eyebrow">WordsNote Governance</p>
      <h1>{{ currentCopy.title }}</h1>
      <p class="lead">{{ currentCopy.subtitle }}</p>

      <div class="lang-switch" role="group" :aria-label="currentCopy.langAriaLabel">
        <button :class="['lang-btn', { active: language === 'vi' }]" @click="setLanguage('vi')">VI</button>
        <button :class="['lang-btn', { active: language === 'en' }]" @click="setLanguage('en')">EN</button>
      </div>

      <p class="updated">{{ currentCopy.updatedLabel }}: {{ UPDATED_AT }}</p>
    </header>

    <article class="privacy-card">
      <section v-for="section in currentCopy.sections" :key="section.id" class="privacy-section">
        <h2>{{ section.heading }}</h2>
        <p v-if="section.intro" class="section-intro">{{ section.intro }}</p>
        <ul>
          <li v-for="item in section.items" :key="item">{{ item }}</li>
        </ul>
      </section>

      <section class="privacy-section">
        <h2>{{ currentCopy.contactHeading }}</h2>
        <p class="section-intro">{{ currentCopy.contactText }}</p>
      </section>
    </article>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

type LanguageMode = 'vi' | 'en'

interface PolicySection {
  id: string
  heading: string
  intro?: string
  items: string[]
}

interface PolicyCopy {
  title: string
  subtitle: string
  updatedLabel: string
  langAriaLabel: string
  contactHeading: string
  contactText: string
  sections: PolicySection[]
}

const UPDATED_AT = '2026-04-17'

const route = useRoute()
const router = useRouter()

const language = ref<LanguageMode>('vi')

const policyCopy: Record<LanguageMode, PolicyCopy> = {
 vi: {
    title: 'Chính sách bảo mật',
    subtitle:
      'Trang này mô tả cách WordsNote thu thập, sử dụng, lưu trữ và bảo vệ dữ liệu người dùng trên web app và extension.',
    updatedLabel: 'Cập nhật lần cuối',
    langAriaLabel: 'Chọn ngôn ngữ hiển thị',
    contactHeading: 'Liên hệ về quyền riêng tư',
    contactText:
      'Nếu bạn có yêu cầu liên quan đến dữ liệu cá nhân, vui lòng liên hệ email quản trị được cấu hình trên hệ thống triển khai hiện tại.',
    sections: [
      {
        id: 'scope',
        heading: '1. Phạm vi áp dụng',
        items: [
          'Áp dụng cho website WordsNote và extension trình duyệt WordsNote.',
          'Áp dụng cho các tính năng học tập, quản lý bộ thẻ và đồng bộ dữ liệu local/cloud trong phạm vi sản phẩm.',
        ],
      },
      {
        id: 'data-collected',
        heading: '2. Dữ liệu được thu thập',
        items: [
          'Thông tin đăng nhập Google trên khu vực Manage của web app (nếu bạn sử dụng tính năng này).',
          'Nội dung bộ sưu tập (collection) và thẻ (card) do bạn tạo trên web app.',
          'Dữ liệu cục bộ (local) trong extension, bao gồm nội dung làm nổi bật (highlight) và tiến độ học được lưu trong chrome.storage.local.',
          'Thông tin kỹ thuật tối thiểu cần thiết để hệ thống hoạt động ổn định và bảo mật.',
        ],
      },
      {
        id: 'usage',
        heading: '3. Mục đích sử dụng dữ liệu',
        items: [
          'Cung cấp các chế độ học: Flashcards, Learn, Practice.',
          'Đồng bộ và hiển thị dữ liệu bộ sưu tập/thẻ từ API công khai khi bạn sử dụng tính năng Cloud Sync.',
          'Cải thiện độ ổn định, bảo mật và chất lượng trải nghiệm sản phẩm.',
        ],
      },
      {
        id: 'storage',
        heading: '4. Lưu trữ và đồng bộ dữ liệu',
        items: [
          'Web app lưu dữ liệu học tập trên backend và có thể sử dụng cơ sở dữ liệu máy chủ.',
          'Extension có thể hoạt động ở chế độ local, khi đó dữ liệu nằm trên trình duyệt của bạn.',
          'Khi bạn bấm "Sync To Local" trong chế độ Cloud, dữ liệu thẻ từ cloud sẽ được sao chép về máy cục bộ để học ở chế độ Local.',
        ],
      },
      {
        id: 'sharing',
        heading: '5. Chia sẻ dữ liệu',
        items: [
          'Chúng tôi không bán dữ liệu cá nhân cho bên thứ ba.',
          'Dữ liệu chỉ được chia sẻ trong các trường hợp cần thiết để vận hành dịch vụ hoặc theo yêu cầu pháp lý hợp lệ.',
        ],
      },
      {
        id: 'security',
        heading: '6. Bảo mật dữ liệu',
        items: [
          'Hệ thống áp dụng các biện pháp kỹ thuật hợp lý để bảo vệ dữ liệu khỏi truy cập trái phép.',
          'Không có hệ thống nào an toàn tuyệt đối; người dùng nên chủ động bảo vệ tài khoản và thiết bị của mình.',
        ],
      },
      {
        id: 'rights',
        heading: '7. Quyền của người dùng',
        items: [
          'Bạn có thể xem, chỉnh sửa hoặc xóa dữ liệu học tập của mình trong phạm vi tính năng hỗ trợ.',
          'Bạn có thể xóa dữ liệu extension local bằng cách xóa bộ nhớ lưu trữ (storage) của extension trên trình duyệt.',
          'Bạn có thể gửi yêu cầu liên quan đến dữ liệu cá nhân thông qua kênh liên hệ quản trị.',
        ],
      },
      {
        id: 'changes',
        heading: '8. Thay đổi chính sách',
        items: [
          'Chính sách này có thể được cập nhật theo thay đổi sản phẩm hoặc yêu cầu pháp lý.',
          'Ngày cập nhật mới nhất sẽ được hiển thị ở đầu trang này.',
        ],
      },
    ],
},
  en: {
    title: 'Privacy Policy',
    subtitle:
      'This page describes how WordsNote collects, uses, stores, and protects user data across the web app and browser extension.',
    updatedLabel: 'Last updated',
    langAriaLabel: 'Choose display language',
    contactHeading: 'Privacy Contact',
    contactText:
      'If you have any request related to personal data, please contact the administrator email configured in the current deployment environment.',
    sections: [
      {
        id: 'scope',
        heading: '1. Scope',
        items: [
          'Applies to the WordsNote web application and WordsNote browser extension.',
          'Applies to learning features, collection management, and local/cloud sync capabilities within the product.',
        ],
      },
      {
        id: 'data-collected',
        heading: '2. Data We Collect',
        items: [
          'Google sign-in information in the Manage area of the web app (if you use that feature).',
          'Collection/card content that you create in the web app.',
          'Extension local data, including highlighted text and learning progress stored in chrome.storage.local.',
          'Minimum technical data required for secure and stable system operation.',
        ],
      },
      {
        id: 'usage',
        heading: '3. How We Use Data',
        items: [
          'To provide learning modes: Flashcards, Learn, and Practice.',
          'To sync and display collection/card data from public APIs when you use Cloud Sync.',
          'To improve reliability, security, and overall product quality.',
        ],
      },
      {
        id: 'storage',
        heading: '4. Data Storage and Sync',
        items: [
          'The web app stores learning data on the backend and may use server-side databases.',
          'The extension can operate in local mode where data remains in your browser.',
          'When you click Sync To Local in Cloud mode, cloud card data is copied into local storage for Local mode learning.',
        ],
      },
      {
        id: 'sharing',
        heading: '5. Data Sharing',
        items: [
          'We do not sell personal data to third parties.',
          'Data is shared only when required to operate the service or to comply with lawful obligations.',
        ],
      },
      {
        id: 'security',
        heading: '6. Data Security',
        items: [
          'Reasonable technical measures are used to protect data from unauthorized access.',
          'No system is absolutely secure; users should also protect their own accounts and devices.',
        ],
      },
      {
        id: 'rights',
        heading: '7. Your Rights',
        items: [
          'You can review, edit, or remove your learning data within supported product features.',
          'You can remove extension local data by clearing extension storage in your browser.',
          'You can submit data-related requests through the administrator contact channel.',
        ],
      },
      {
        id: 'changes',
        heading: '8. Policy Updates',
        items: [
          'This policy may be updated when product behavior or legal requirements change.',
          'The latest update date will always be shown at the top of this page.',
        ],
      },
    ],
  },
}

const currentCopy = computed(() => policyCopy[language.value])

function parseLanguageQuery(rawLang: unknown): LanguageMode | null {
  const value = Array.isArray(rawLang) ? rawLang[0] : rawLang
  if (value === 'vi' || value === 'en') {
    return value
  }

  return null
}

function syncUrlLanguage(nextLanguage: LanguageMode) {
  const currentLang = parseLanguageQuery(route.query.lang)
  if (currentLang === nextLanguage) {
    return
  }

  void router.replace({
    query: {
      ...route.query,
      lang: nextLanguage,
    },
  })
}

const initialLanguage = parseLanguageQuery(route.query.lang)
if (initialLanguage) {
  language.value = initialLanguage
} else {
  syncUrlLanguage(language.value)
}

watch(
  () => route.query.lang,
  (nextLang) => {
    const parsed = parseLanguageQuery(nextLang)
    if (!parsed) {
      syncUrlLanguage(language.value)
      return
    }

    if (parsed !== language.value) {
      language.value = parsed
    }
  },
)

watch(language, (nextLanguage) => {
  syncUrlLanguage(nextLanguage)
})

function setLanguage(mode: LanguageMode) {
  language.value = mode
}
</script>

<style scoped>
.privacy-wrap {
  max-width: 980px;
  margin: 0 auto;
  padding: 2rem 1rem 3rem;
  display: grid;
  gap: 1rem;
}

.privacy-hero {
  border: 1px solid var(--wn-border);
  border-radius: 24px;
  padding: 1.4rem 1.3rem;
  background:
    radial-gradient(140% 120% at 0% 0%, rgba(255, 188, 119, 0.24), transparent 46%),
    radial-gradient(90% 120% at 100% 100%, rgba(100, 173, 255, 0.2), transparent 50%),
    var(--wn-surface-soft);
  box-shadow: var(--wn-shadow-soft);
}

.eyebrow {
  margin: 0 0 0.35rem;
  text-transform: uppercase;
  letter-spacing: 0.16em;
  font-size: 0.72rem;
  color: var(--wn-muted);
}

h1 {
  margin: 0;
  line-height: 1.12;
}

.lead {
  margin: 0.7rem 0 0;
  color: var(--wn-muted);
  max-width: 78ch;
}

.updated {
  margin: 0.65rem 0 0;
  color: var(--wn-muted);
  font-size: 0.9rem;
}

.lang-switch {
  margin-top: 1rem;
  display: inline-flex;
  border: 1px solid var(--wn-border);
  border-radius: 999px;
  overflow: hidden;
  background: var(--wn-surface);
}

.lang-btn {
  border: 0;
  background: transparent;
  color: var(--wn-ink);
  padding: 0.42rem 0.84rem;
  font-weight: 600;
}

.lang-btn.active {
  background: var(--wn-primary);
  color: var(--wn-on-primary);
}

.privacy-card {
  border: 1px solid var(--wn-border);
  border-radius: 20px;
  background: var(--wn-surface);
  box-shadow: var(--wn-shadow-soft);
  padding: 1rem 1.1rem;
  display: grid;
  gap: 1rem;
}

.privacy-section h2 {
  margin: 0;
  font-size: 1.1rem;
}

.section-intro {
  margin: 0.42rem 0 0;
  color: var(--wn-muted);
}

ul {
  margin: 0.55rem 0 0;
  padding-left: 1.2rem;
  display: grid;
  gap: 0.36rem;
  color: var(--wn-ink);
}

@media (max-width: 720px) {
  .privacy-wrap {
    padding-top: 1.45rem;
  }

  .privacy-hero,
  .privacy-card {
    border-radius: 16px;
  }
}
</style>
