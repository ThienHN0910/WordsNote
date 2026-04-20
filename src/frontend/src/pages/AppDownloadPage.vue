<template>
  <section class="download-wrap">
    <article class="hero">
      <p class="eyebrow">Desktop App</p>
      <h1>{{ displayTitle }}</h1>
      <p class="summary">{{ displaySummary }}</p>

      <div class="meta-row">
        <span v-if="displayVersion">Version: {{ displayVersion }}</span>
        <span v-if="displayPublishedAt">Published: {{ displayPublishedAt }}</span>
      </div>

      <div class="actions">
        <a
          v-if="displayDownloadUrl"
          class="cta primary"
          :href="displayDownloadUrl"
          target="_blank"
          rel="noopener noreferrer"
        >
          Download for Windows
        </a>
        <a
          v-if="displayReleaseNotesUrl"
          class="cta ghost"
          :href="displayReleaseNotesUrl"
          target="_blank"
          rel="noopener noreferrer"
        >
          Release details
        </a>
        <a
          v-if="displayChecksumUrl"
          class="cta ghost"
          :href="displayChecksumUrl"
          target="_blank"
          rel="noopener noreferrer"
        >
          Checksums
        </a>
      </div>

      <p v-if="displayAssetName" class="asset-name">Primary package: {{ displayAssetName }}</p>
      <p v-if="loadError" class="error">{{ loadError }}</p>
      <p v-if="isLoading" class="hint">Loading latest release data...</p>
    </article>

    <article class="asset-list" v-if="releaseSnapshot?.assets.length">
      <h2>Release assets</h2>
      <ul>
        <li v-for="asset in releaseSnapshot.assets" :key="asset.name">
          <a :href="asset.browserDownloadUrl" target="_blank" rel="noopener noreferrer">{{ asset.name }}</a>
          <span>{{ formatFileSize(asset.size) }}</span>
          <span>{{ asset.downloadCount }} downloads</span>
        </li>
      </ul>
    </article>

    <article class="editor" v-if="canEdit">
      <h2>Edit download page data</h2>
      <p class="hint">Signed in as {{ editorEmail }}. Changes are saved to this browser.</p>

      <form class="editor-grid" @submit.prevent="saveOverrides">
        <label>
          Title
          <input v-model="overrideForm.title" type="text" placeholder="Words note desktop download" />
        </label>

        <label>
          Summary
          <textarea
            v-model="overrideForm.summary"
            rows="3"
            placeholder="Download the latest installer and start learning on Windows."
          ></textarea>
        </label>

        <label>
          Primary download URL
          <input v-model="overrideForm.primaryDownloadUrl" type="url" placeholder="https://.../WordsNote.msixbundle" />
        </label>

        <label>
          Primary asset name
          <input v-model="overrideForm.primaryAssetName" type="text" placeholder="WordsNote.Package_1.1.2.0_x64_arm64.msixbundle" />
        </label>

        <label>
          Version label
          <input v-model="overrideForm.versionLabel" type="text" placeholder="1.1.2" />
        </label>

        <label>
          Published date text
          <input v-model="overrideForm.publishedAt" type="text" placeholder="20 Apr 2026" />
        </label>

        <label>
          Release notes URL
          <input v-model="overrideForm.releaseNotesUrl" type="url" placeholder="https://github.com/.../releases/tag/v1.1.2" />
        </label>

        <label>
          Checksum URL
          <input v-model="overrideForm.checksumUrl" type="url" placeholder="https://github.com/.../SHA256SUMS.txt" />
        </label>

        <div class="editor-actions">
          <button class="btn-save" type="submit">Save</button>
          <button class="btn-clear" type="button" @click="clearOverrides">Reset</button>
        </div>
      </form>
    </article>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { AppDownloadService } from '@/services/FFP/AppDownloadService'
import { UserService } from '@/services/AS/UserService'
import type { DownloadPageOverride, GithubReleaseSnapshot } from '@/types/FFP/AppDownload'

const authStore = useAuthStore()

const releaseSnapshot = ref<GithubReleaseSnapshot | null>(null)
const isLoading = ref(false)
const loadError = ref('')
const editorEmail = ref('')

const githubRepo = (import.meta.env.VITE_GITHUB_REPO || 'ThinHN/WordsNote').trim()
const configuredEditorEmail = (import.meta.env.VITE_DOWNLOAD_EDITOR_EMAIL || '').trim().toLowerCase()

const overrideForm = reactive<DownloadPageOverride>({
  ...AppDownloadService.loadOverride(),
})

const hasAuthSession = computed(() => authStore.hasAuthSession)
const canEdit = computed(() => {
  const email = editorEmail.value.trim().toLowerCase()
  if (!email) {
    return false
  }

  if (!configuredEditorEmail) {
    return true
  }

  return email === configuredEditorEmail
})

const preferredAsset = computed(() => {
  const assets = releaseSnapshot.value?.assets ?? []
  return AppDownloadService.findPreferredInstallerAsset(assets)
})

const displayTitle = computed(() => {
  if (overrideForm.title) {
    return overrideForm.title
  }

  const fallbackName = releaseSnapshot.value?.name || releaseSnapshot.value?.tagName
  return fallbackName ? `Words note ${fallbackName}` : 'Words note desktop download'
})

const displaySummary = computed(() => {
  if (overrideForm.summary) {
    return overrideForm.summary
  }

  return 'Download the latest Windows installer for Words note and continue learning with Flashcards, Learn, and Practice modes.'
})

const displayVersion = computed(() => {
  if (overrideForm.versionLabel) {
    return overrideForm.versionLabel
  }

  const tag = releaseSnapshot.value?.tagName || ''
  return tag.replace(/^v/i, '')
})

const displayPublishedAt = computed(() => {
  if (overrideForm.publishedAt) {
    return overrideForm.publishedAt
  }

  const publishedAt = releaseSnapshot.value?.publishedAt
  if (!publishedAt) {
    return ''
  }

  const date = new Date(publishedAt)
  return Number.isNaN(date.getTime()) ? publishedAt : date.toLocaleDateString()
})

const displayDownloadUrl = computed(() => {
  if (overrideForm.primaryDownloadUrl) {
    return overrideForm.primaryDownloadUrl
  }

  return preferredAsset.value?.browserDownloadUrl || ''
})

const displayAssetName = computed(() => {
  if (overrideForm.primaryAssetName) {
    return overrideForm.primaryAssetName
  }

  return preferredAsset.value?.name || ''
})

const displayReleaseNotesUrl = computed(() => {
  if (overrideForm.releaseNotesUrl) {
    return overrideForm.releaseNotesUrl
  }

  return releaseSnapshot.value?.htmlUrl || ''
})

const displayChecksumUrl = computed(() => overrideForm.checksumUrl || '')

async function loadLatestRelease() {
  isLoading.value = true
  loadError.value = ''

  try {
    releaseSnapshot.value = await AppDownloadService.fetchLatestRelease(githubRepo)
  } catch (error) {
    loadError.value = error instanceof Error ? error.message : 'Failed to load release data.'
  } finally {
    isLoading.value = false
  }
}

async function loadEditorIdentity() {
  if (!hasAuthSession.value) {
    editorEmail.value = ''
    return
  }

  try {
    const response = await UserService.getMyProfile()
    const payload = (response.data ?? {}) as Record<string, unknown>
    editorEmail.value = String(payload.email ?? payload.Email ?? '').trim()
  } catch {
    editorEmail.value = ''
  }
}

function saveOverrides() {
  AppDownloadService.saveOverride(overrideForm)
}

function clearOverrides() {
  AppDownloadService.clearOverride()
  const cleared = AppDownloadService.loadOverride()
  overrideForm.title = cleared.title
  overrideForm.summary = cleared.summary
  overrideForm.primaryDownloadUrl = cleared.primaryDownloadUrl
  overrideForm.primaryAssetName = cleared.primaryAssetName
  overrideForm.checksumUrl = cleared.checksumUrl
  overrideForm.releaseNotesUrl = cleared.releaseNotesUrl
  overrideForm.versionLabel = cleared.versionLabel
  overrideForm.publishedAt = cleared.publishedAt
}

function formatFileSize(bytes: number) {
  if (!Number.isFinite(bytes) || bytes <= 0) {
    return '0 B'
  }

  const units = ['B', 'KB', 'MB', 'GB']
  let size = bytes
  let unitIndex = 0

  while (size >= 1024 && unitIndex < units.length - 1) {
    size /= 1024
    unitIndex += 1
  }

  const rounded = unitIndex === 0 ? size.toFixed(0) : size.toFixed(2)
  return `${rounded} ${units[unitIndex]}`
}

watch(hasAuthSession, () => {
  void loadEditorIdentity()
}, { immediate: true })

onMounted(() => {
  authStore.rehydrateFromPersistedState()
  void loadLatestRelease()
})
</script>

<style scoped>
.download-wrap {
  max-width: 1120px;
  margin: 0 auto;
  padding: 2rem 1rem 3rem;
  display: grid;
  gap: 1rem;
}

.hero,
.asset-list,
.editor {
  border: 1px solid var(--wn-border);
  border-radius: 18px;
  background: var(--wn-surface);
  box-shadow: var(--wn-shadow-soft);
  padding: 1.2rem;
}

.hero {
  background:
    radial-gradient(110% 130% at 0% 0%, color-mix(in srgb, var(--wn-primary) 14%, transparent), transparent 52%),
    var(--wn-surface-soft);
}

.eyebrow {
  margin: 0 0 0.45rem;
  text-transform: uppercase;
  letter-spacing: 0.16em;
  font-size: 0.72rem;
  color: var(--wn-muted);
}

h1 {
  margin: 0;
  font-size: clamp(1.65rem, 2.8vw, 2.3rem);
}

h2 {
  margin: 0 0 0.7rem;
}

.summary {
  margin-top: 0.8rem;
  color: var(--wn-muted);
  max-width: 70ch;
}

.meta-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.65rem;
  margin-top: 0.7rem;
  color: var(--wn-muted);
  font-size: 0.93rem;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.65rem;
  margin-top: 1rem;
}

.cta {
  text-decoration: none;
  padding: 0.6rem 0.95rem;
  border-radius: 12px;
  border: 1px solid transparent;
  font-weight: 600;
}

.cta.primary {
  background: var(--wn-primary);
  color: var(--wn-on-primary);
}

.cta.ghost {
  border-color: var(--wn-border);
  color: var(--wn-ink);
  background: var(--wn-surface);
}

.asset-name {
  margin-top: 0.8rem;
  color: var(--wn-muted);
  font-size: 0.93rem;
}

.hint {
  margin-top: 0.8rem;
  color: var(--wn-muted);
}

.error {
  margin-top: 0.8rem;
  color: #c0392b;
}

.asset-list ul {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.5rem;
}

.asset-list li {
  display: grid;
  grid-template-columns: minmax(220px, 1fr) auto auto;
  gap: 0.5rem;
  align-items: center;
  border: 1px solid var(--wn-border);
  border-radius: 10px;
  padding: 0.5rem 0.7rem;
}

.asset-list a {
  color: var(--wn-link);
  text-decoration: none;
  overflow-wrap: anywhere;
}

.editor-grid {
  display: grid;
  gap: 0.65rem;
}

.editor-grid label {
  display: grid;
  gap: 0.28rem;
  font-size: 0.92rem;
}

.editor-grid input,
.editor-grid textarea {
  border: 1px solid var(--wn-border);
  background: var(--wn-surface-soft);
  color: var(--wn-ink);
  border-radius: 10px;
  padding: 0.55rem 0.65rem;
}

.editor-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.btn-save,
.btn-clear {
  border: 1px solid var(--wn-border);
  border-radius: 10px;
  padding: 0.5rem 0.9rem;
  background: var(--wn-surface);
  color: var(--wn-ink);
}

.btn-save {
  background: var(--wn-primary);
  border-color: var(--wn-primary);
  color: var(--wn-on-primary);
}

@media (max-width: 820px) {
  .asset-list li {
    grid-template-columns: 1fr;
  }
}
</style>
