<template>
  <section class="download-wrap">
    <header class="hero-shell">
      <div class="hero-main">
        <p class="eyebrow">Desktop Distribution</p>
        <h1>{{ displayTitle }}</h1>
        <p class="summary">{{ displaySummary }}</p>
      </div>

      <div class="hero-meta">
        <div class="meta-item">
          <span class="meta-label">Repository</span>
          <strong>{{ sourceRepo }}</strong>
        </div>
        <div class="meta-item">
          <span class="meta-label">Versions loaded</span>
          <strong>{{ visibleReleases.length }}</strong>
        </div>
        <div class="meta-item" v-if="selectedRelease">
          <span class="meta-label">Selected</span>
          <strong>{{ selectedRelease.tagName || selectedRelease.name }}</strong>
        </div>
      </div>
    </header>

    <p v-if="loadError" class="error-banner">{{ loadError }}</p>
    <p v-if="isLoading" class="loading-banner">Loading release versions from GitHub...</p>

    <section class="channels" v-if="channelCards.length">
      <aside class="channel-rail">
        <h2>Get App or Extension</h2>
        <p class="channel-rail-sub">Store links and custom links are displayed separately from GitHub release assets.</p>
        <ul>
          <li v-for="channel in channelCards" :key="channel.id">
            <button
              type="button"
              class="channel-button"
              :class="{ active: isSelectedChannel(channel.id) }"
              @click="selectChannel(channel.id)"
            >
              <span class="version-name">{{ channel.title }}</span>
              <span class="version-date">{{ channel.kicker }}</span>
              <span class="version-links">{{ channel.links.length }} links</span>
            </button>
          </li>
        </ul>
      </aside>

      <article class="channel-panel" v-if="selectedChannel">
        <header class="release-header">
          <div>
            <p class="release-kicker">Selected channel</p>
            <h2>{{ selectedChannel.title }}</h2>
            <p class="release-sub">{{ selectedChannel.kicker }} • {{ selectedChannel.links.length }} links</p>
          </div>
        </header>

        <div class="link-grid" v-if="selectedChannel.links.length">
          <a
            v-for="link in selectedChannel.links"
            :key="`${selectedChannel.id}-${link.url}-${link.name}`"
            class="download-link-card"
            :href="link.url"
            target="_blank"
            rel="noopener noreferrer"
          >
            <div class="link-top">
              <span class="kind" :data-kind="link.kind">{{ kindLabel(link.kind) }}</span>
            </div>
            <strong class="link-name">{{ link.name }}</strong>
            <span class="channels-cta-url">{{ link.url }}</span>
          </a>
        </div>

        <p v-else class="empty-state">No links configured for this channel.</p>
      </article>
    </section>

    <section class="workspace" v-if="visibleReleases.length">
      <aside class="version-rail">
        <h2>GitHub Versions</h2>
        <ul>
          <li v-for="release in visibleReleases" :key="release.tagName || release.name">
            <button
              type="button"
              class="version-button"
              :class="{ active: isSelectedRelease(release.tagName) }"
              @click="selectRelease(release.tagName)"
            >
              <span class="version-name">{{ release.tagName || release.name }}</span>
              <span class="version-date">{{ formatPublishedAt(release.publishedAt) }}</span>
              <span class="version-links">{{ getGithubReleaseLinkCount(release.tagName) }} assets</span>
            </button>
          </li>
        </ul>
      </aside>

      <article class="release-panel" v-if="selectedRelease">
        <header class="release-header">
          <div>
            <p class="release-kicker">Selected GitHub version</p>
            <h2>{{ selectedRelease.name || selectedRelease.tagName }}</h2>
            <p class="release-sub">
              {{ selectedRelease.tagName }} • {{ formatPublishedAt(selectedRelease.publishedAt) }}
            </p>
          </div>

          <a
            v-if="selectedRelease.htmlUrl"
            class="release-link"
            :href="selectedRelease.htmlUrl"
            target="_blank"
            rel="noopener noreferrer"
          >
            Open release page
          </a>
        </header>

        <div class="link-grid" v-if="selectedReleaseLinks.length">
          <a
            v-for="link in selectedReleaseLinks"
            :key="link.url"
            class="download-link-card"
            :href="link.url"
            target="_blank"
            rel="noopener noreferrer"
          >
            <div class="link-top">
              <span class="kind" :data-kind="link.kind">{{ kindLabel(link.kind) }}</span>
              <span class="downloads">{{ link.downloadCount }} downloads</span>
            </div>
            <strong class="link-name">{{ link.name }}</strong>
            <span class="link-size">{{ formatFileSize(link.size) }}</span>
          </a>
        </div>

        <p v-else class="empty-state">No downloadable GitHub assets found for this version.</p>
      </article>
    </section>

    <AppDownloadCatalogSection v-if="releaseCards.length" :release-cards="releaseCards" />

    <article class="editor" v-if="canEdit">
      <h2>Edit download page settings</h2>
      <p class="hint">Signed in as {{ editorEmail }}. Settings are shared from backend for all devices.</p>

      <p v-if="saveMessage" class="success-banner">{{ saveMessage }}</p>
      <p v-if="saveError" class="error-banner">{{ saveError }}</p>

      <form class="editor-grid" @submit.prevent="saveOverrides">
        <label>
          Page title
          <input v-model="overrideForm.title" type="text" placeholder="Words note desktop downloads" />
        </label>

        <label>
          Summary
          <textarea
            v-model="overrideForm.summary"
            rows="3"
            placeholder="Choose any version and download the installer package you need."
          ></textarea>
        </label>

        <label>
          App Store URL
          <input v-model="overrideForm.appStoreUrl" type="url" placeholder="https://apps.microsoft.com/..." />
        </label>

        <label>
          Edge Add-ons URL
          <input v-model="overrideForm.edgeAddonsUrl" type="url" placeholder="https://microsoftedge.microsoft.com/addons/..." />
        </label>

        <label>
          GitHub repository
          <input v-model="overrideForm.repo" type="text" placeholder="ThienHN0910/WordsNote" />
        </label>

        <label>
          Max visible versions
          <input v-model.number="overrideForm.maxVisibleVersions" type="number" min="1" max="30" />
        </label>

        <label>
          Featured version tag (optional)
          <input v-model="overrideForm.featuredTag" type="text" placeholder="v1.1.2" />
        </label>

        <fieldset class="manual-section">
          <legend>Custom links per version</legend>

          <div class="manual-input-grid">
            <label>
              Version tag
              <input v-model="manualLinkDraft.tagName" type="text" placeholder="v1.1.2" />
            </label>

            <label>
              Link name
              <input v-model="manualLinkDraft.name" type="text" placeholder="Desktop installer mirror" />
            </label>

            <label>
              Link URL
              <input v-model="manualLinkDraft.url" type="url" placeholder="https://example.com/download" />
            </label>

            <label>
              Type
              <select v-model="manualLinkDraft.kind">
                <option value="installer">Installer</option>
                <option value="archive">Archive</option>
                <option value="other">Other</option>
              </select>
            </label>

            <button class="btn-add" type="button" @click="addManualLink">Add custom link</button>
          </div>

          <div class="manual-list" v-if="manualLinkGroups.length">
            <article class="manual-group" v-for="group in manualLinkGroups" :key="group.tagName">
              <header>
                <h3>{{ group.tagName }}</h3>
                <span>{{ group.links.length }} links</span>
              </header>

              <ul>
                <li v-for="(link, linkIndex) in group.links" :key="`${group.tagName}-${link.url}-${linkIndex}`">
                  <div class="manual-link-body">
                    <strong>{{ link.name }}</strong>
                    <a :href="link.url" target="_blank" rel="noopener noreferrer">{{ link.url }}</a>
                  </div>

                  <span class="kind" :data-kind="link.kind">{{ kindLabel(link.kind) }}</span>
                  <button type="button" class="btn-remove" @click="removeManualLink(group.tagName, linkIndex)">
                    Remove
                  </button>
                </li>
              </ul>
            </article>
          </div>

          <p v-else class="hint">No custom links yet.</p>
        </fieldset>

        <div class="editor-actions">
          <button class="btn-save" type="submit" :disabled="isSavingSettings || isResettingSettings">
            {{ isSavingSettings ? 'Saving...' : 'Save settings' }}
          </button>
          <button class="btn-clear" type="button" :disabled="isSavingSettings || isResettingSettings" @click="clearOverrides">
            {{ isResettingSettings ? 'Resetting...' : 'Reset' }}
          </button>
        </div>
      </form>
    </article>
  </section>
</template>

<script setup lang="ts">
import { computed, defineAsyncComponent, onMounted, reactive, ref, watch } from 'vue'
import { useAuthStore } from '@/stores/AS/AuthStore'
import { AppDownloadService } from '@/services/FFP/AppDownloadService'
import { UserService } from '@/services/AS/UserService'
import AppLoading from '@/components/ui/AppLoading.vue'
import type {
  DownloadPageOverride,
  GithubReleaseSnapshot,
  ManualReleaseDownloadLink,
  ManualReleaseLinksByVersion,
  ReleaseDownloadKind,
  ReleaseDownloadLink,
} from '@/types/FFP/AppDownload'

interface ReleaseCard {
  release: GithubReleaseSnapshot
  links: ReleaseDownloadLink[]
}

const AppDownloadCatalogSection = defineAsyncComponent({
  loader: () => import('@/components/FFP/AppDownloadCatalogSection.vue'),
  loadingComponent: AppLoading,
  delay: 120,
  suspensible: false,
})

interface QuickAccessLink {
  title: string
  kicker: string
  url: string
  kind: ReleaseDownloadKind
}

interface ChannelDisplayLink {
  name: string
  url: string
  kind: ReleaseDownloadKind
}

interface DownloadChannelCard {
  id: string
  title: string
  kicker: string
  links: ChannelDisplayLink[]
}

interface ManualLinkDraft {
  tagName: string
  name: string
  url: string
  kind: ReleaseDownloadKind
}

const authStore = useAuthStore()

const releaseSnapshots = ref<GithubReleaseSnapshot[]>([])
const isLoading = ref(false)
const loadError = ref('')
const saveMessage = ref('')
const saveError = ref('')
const isSavingSettings = ref(false)
const isResettingSettings = ref(false)
const editorEmail = ref('')
const editorRole = ref('')
const selectedTag = ref('')
const selectedChannelId = ref('')
const lastLoadedRepo = ref('')

const fallbackRepo = 'ThienHN0910/WordsNote'
const allowedAdminEmail = (import.meta.env.VITE_GOOGLE_ALLOWED_EMAIL || '').trim().toLowerCase()

const overrideForm = reactive<DownloadPageOverride>({
  title: '',
  summary: '',
  appStoreUrl: '',
  edgeAddonsUrl: '',
  repo: '',
  maxVisibleVersions: 8,
  featuredTag: '',
  manualLinksByVersion: [],
})

const manualLinkDraft = reactive<ManualLinkDraft>({
  tagName: '',
  name: '',
  url: '',
  kind: 'installer',
})

const hasAuthSession = computed(() => authStore.hasAuthSession)
const sourceRepo = computed(() => {
  const candidate = String(overrideForm.repo || '').trim()
  return candidate || fallbackRepo
})

const maxVisibleVersions = computed(() => {
  const numeric = Number(overrideForm.maxVisibleVersions)
  if (!Number.isFinite(numeric)) {
    return 8
  }

  return Math.min(30, Math.max(1, Math.trunc(numeric)))
})

const canEdit = computed(() => {
  if (!hasAuthSession.value) {
    return false
  }

  const email = editorEmail.value.trim().toLowerCase()
  if (!email) {
    return false
  }

  const role = editorRole.value.trim().toLowerCase()
  if (role === 'admin') {
    return true
  }

  if (!allowedAdminEmail) {
    return false
  }

  return email === allowedAdminEmail
})

const visibleReleases = computed(() => releaseSnapshots.value.slice(0, maxVisibleVersions.value))

const releaseCards = computed<ReleaseCard[]>(() => {
  return visibleReleases.value.map((release) => ({
    release,
    links: AppDownloadService.buildReleaseLinks(release.assets),
  }))
})

const manualLinkGroups = computed(() => {
  return sanitizeManualLinksByVersion(overrideForm.manualLinksByVersion)
    .sort((a, b) => b.tagName.localeCompare(a.tagName, undefined, { numeric: true, sensitivity: 'base' }))
})

const quickAccessLinks = computed<QuickAccessLink[]>(() => {
  const result: QuickAccessLink[] = []
  const appStoreUrl = normalizeText(overrideForm.appStoreUrl)
  const edgeAddonsUrl = normalizeText(overrideForm.edgeAddonsUrl)

  if (appStoreUrl) {
    result.push({
      title: 'Get App',
      kicker: 'Microsoft Store',
      url: appStoreUrl,
      kind: 'installer',
    })
  }

  if (edgeAddonsUrl) {
    result.push({
      title: 'Get Extension',
      kicker: 'Microsoft Edge Add-ons',
      url: edgeAddonsUrl,
      kind: 'other',
    })
  }

  return result
})

const channelCards = computed<DownloadChannelCard[]>(() => {
  const cards: DownloadChannelCard[] = []

  for (const item of quickAccessLinks.value) {
    cards.push({
      id: `cta-${item.title}-${item.kicker}`.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
      title: item.title,
      kicker: item.kicker,
      links: [
        {
          name: item.title,
          url: item.url,
          kind: item.kind,
        },
      ],
    })
  }

  for (const group of manualLinkGroups.value) {
    cards.push({
      id: `manual-${group.tagName}`.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
      title: group.tagName,
      kicker: 'Custom links',
      links: group.links.map((link) => ({
        name: link.name,
        url: link.url,
        kind: link.kind,
      })),
    })
  }

  return cards
})

const selectedChannel = computed(() => {
  if (!channelCards.value.length) {
    return null
  }

  const matched = channelCards.value.find((item) => item.id === selectedChannelId.value)
  return matched || channelCards.value[0]
})

const selectedRelease = computed(() => {
  if (!visibleReleases.value.length) {
    return null
  }

  const featuredTag = String(overrideForm.featuredTag || '').trim()
  if (featuredTag) {
    const featured = visibleReleases.value.find((release) => release.tagName === featuredTag)
    if (featured) {
      return featured
    }
  }

  if (selectedTag.value) {
    const active = visibleReleases.value.find((release) => release.tagName === selectedTag.value)
    if (active) {
      return active
    }
  }

  return visibleReleases.value[0]
})

const selectedReleaseLinks = computed(() => {
  if (!selectedRelease.value) {
    return [] as ReleaseDownloadLink[]
  }

  return AppDownloadService.buildReleaseLinks(selectedRelease.value.assets)
})

const displayTitle = computed(() => {
  if (overrideForm.title) {
    return String(overrideForm.title).trim()
  }

  return 'Words note desktop downloads'
})

const displaySummary = computed(() => {
  if (overrideForm.summary) {
    return String(overrideForm.summary).trim()
  }

  return 'Browse multiple versions and choose the installer package that matches your setup.'
})

function isSelectedRelease(tagName: string) {
  const selected = selectedRelease.value?.tagName || ''
  return selected === tagName
}

function getGithubReleaseLinkCount(tagName: string) {
  const matched = visibleReleases.value.find((entry) => entry.tagName === tagName)
  if (!matched) {
    return 0
  }

  return AppDownloadService.buildReleaseLinks(matched.assets).length
}

function selectRelease(tagName: string) {
  selectedTag.value = tagName
}

function selectChannel(channelId: string) {
  selectedChannelId.value = channelId
}

function isSelectedChannel(channelId: string) {
  return selectedChannel.value?.id === channelId
}

function addManualLink() {
  const nextTag = normalizeText(manualLinkDraft.tagName)
  const nextName = normalizeText(manualLinkDraft.name)
  const nextUrl = normalizeText(manualLinkDraft.url)

  if (!nextTag || !nextName || !nextUrl) {
    saveError.value = 'Version tag, link name, and URL are required before adding.'
    saveMessage.value = ''
    return
  }

  const normalizedKind = normalizeKind(manualLinkDraft.kind)
  const nextGroups = sanitizeManualLinksByVersion(overrideForm.manualLinksByVersion)
  const normalizedTag = nextTag.toLowerCase()

  let targetGroup = nextGroups.find((group) => group.tagName.toLowerCase() === normalizedTag)
  if (!targetGroup) {
    targetGroup = { tagName: nextTag, links: [] }
    nextGroups.unshift(targetGroup)
  }

  targetGroup.links.unshift({
    name: nextName,
    url: nextUrl,
    kind: normalizedKind,
  })

  overrideForm.manualLinksByVersion = nextGroups

  manualLinkDraft.name = ''
  manualLinkDraft.url = ''
  saveError.value = ''
  saveMessage.value = ''
}

function removeManualLink(tagName: string, linkIndex: number) {
  const normalizedTag = normalizeText(tagName).toLowerCase()
  const nextGroups = sanitizeManualLinksByVersion(overrideForm.manualLinksByVersion)
  const groupIndex = nextGroups.findIndex((group) => group.tagName.toLowerCase() === normalizedTag)
  if (groupIndex < 0) {
    return
  }

  const nextLinks = [...nextGroups[groupIndex].links]
  nextLinks.splice(linkIndex, 1)

  if (!nextLinks.length) {
    nextGroups.splice(groupIndex, 1)
  } else {
    nextGroups[groupIndex] = {
      ...nextGroups[groupIndex],
      links: nextLinks,
    }
  }

  overrideForm.manualLinksByVersion = nextGroups
  saveError.value = ''
  saveMessage.value = ''
}

function kindLabel(kind: ReleaseDownloadKind) {
  switch (kind) {
    case 'installer':
      return 'Installer'
    case 'archive':
      return 'Archive'
    default:
      return 'Asset'
  }
}

function formatPublishedAt(rawValue: string) {
  if (!rawValue) {
    return 'Unknown date'
  }

  const date = new Date(rawValue)
  return Number.isNaN(date.getTime()) ? rawValue : date.toLocaleDateString()
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

  return `${size.toFixed(unitIndex === 0 ? 0 : 2)} ${units[unitIndex]}`
}

async function loadReleases() {
  isLoading.value = true
  loadError.value = ''

  try {
    const snapshots = await AppDownloadService.fetchReleases(sourceRepo.value, 30)
    releaseSnapshots.value = snapshots
    lastLoadedRepo.value = sourceRepo.value

    if (!selectedRelease.value && snapshots.length > 0) {
      selectedTag.value = snapshots[0].tagName
    }
  } catch (error) {
    loadError.value = error instanceof Error ? error.message : 'Failed to load release data.'
  } finally {
    isLoading.value = false
  }
}

async function loadEditorIdentity() {
  if (!hasAuthSession.value) {
    editorEmail.value = ''
    editorRole.value = ''
    return
  }

  try {
    const response = await UserService.getMyProfile()
    const payload = (response.data ?? {}) as Record<string, unknown>
    editorEmail.value = String(payload.email ?? payload.Email ?? '').trim()
    editorRole.value = String(payload.role ?? payload.Role ?? '').trim()
  } catch {
    editorEmail.value = ''
    editorRole.value = ''
  }
}

function applyOverrideToForm(nextOverride: DownloadPageOverride) {
  overrideForm.title = nextOverride.title || ''
  overrideForm.summary = nextOverride.summary || ''
  overrideForm.appStoreUrl = nextOverride.appStoreUrl || ''
  overrideForm.edgeAddonsUrl = nextOverride.edgeAddonsUrl || ''
  overrideForm.repo = nextOverride.repo || ''
  overrideForm.maxVisibleVersions = Number(nextOverride.maxVisibleVersions || 8)
  overrideForm.featuredTag = nextOverride.featuredTag || ''
  overrideForm.manualLinksByVersion = sanitizeManualLinksByVersion(nextOverride.manualLinksByVersion)
}

async function saveOverrides() {
  if (isSavingSettings.value || isResettingSettings.value) {
    return
  }

  isSavingSettings.value = true
  saveError.value = ''
  saveMessage.value = ''

  try {
    const persisted = await AppDownloadService.saveOverride(overrideForm)
    applyOverrideToForm(persisted)
    saveMessage.value = 'Saved shared settings to backend.'

    if (sourceRepo.value !== lastLoadedRepo.value) {
      await loadReleases()
    }
  } catch (error) {
    saveError.value = error instanceof Error ? error.message : 'Failed to save shared settings.'
  } finally {
    isSavingSettings.value = false
  }
}

async function clearOverrides() {
  if (isSavingSettings.value || isResettingSettings.value) {
    return
  }

  isResettingSettings.value = true
  saveError.value = ''
  saveMessage.value = ''

  try {
    await AppDownloadService.clearOverride()
    applyOverrideToForm({ maxVisibleVersions: 8, manualLinksByVersion: [] })
    saveMessage.value = 'Reset shared settings on backend.'

    if (sourceRepo.value !== lastLoadedRepo.value) {
      await loadReleases()
    }
  } catch (error) {
    saveError.value = error instanceof Error ? error.message : 'Failed to reset shared settings.'
  } finally {
    isResettingSettings.value = false
  }
}

function sanitizeManualLinksByVersion(rawValue: unknown): ManualReleaseLinksByVersion[] {
  const groups = Array.isArray(rawValue) ? rawValue : []

  return groups
    .map((rawGroup) => {
      const group = (rawGroup || {}) as ManualReleaseLinksByVersion
      const links = Array.isArray(group.links) ? group.links : []

      return {
        tagName: normalizeText(group.tagName),
        links: links
          .map((rawLink) => {
            const link = (rawLink || {}) as ManualReleaseDownloadLink
            return {
              name: normalizeText(link.name),
              url: normalizeText(link.url),
              kind: normalizeKind(link.kind),
            } as ManualReleaseDownloadLink
          })
          .filter((link) => Boolean(link.name && link.url)),
      } as ManualReleaseLinksByVersion
    })
    .filter((group) => Boolean(group.tagName && group.links.length > 0))
}

function normalizeText(rawValue: unknown) {
  return typeof rawValue === 'string' ? rawValue.trim() : ''
}

function normalizeKind(rawValue: unknown): ReleaseDownloadKind {
  const kind = normalizeText(rawValue).toLowerCase()
  if (kind === 'installer') {
    return 'installer'
  }

  if (kind === 'archive') {
    return 'archive'
  }

  return 'other'
}

watch(visibleReleases, (nextReleases) => {
  if (!nextReleases.length) {
    selectedTag.value = ''
    return
  }

  if (!nextReleases.some((release) => release.tagName === selectedTag.value)) {
    selectedTag.value = nextReleases[0].tagName
  }
})

watch(channelCards, (nextChannels) => {
  if (!nextChannels.length) {
    selectedChannelId.value = ''
    return
  }

  if (!nextChannels.some((channel) => channel.id === selectedChannelId.value)) {
    selectedChannelId.value = nextChannels[0].id
  }
})

watch(hasAuthSession, () => {
  void loadEditorIdentity()
}, { immediate: true })

onMounted(() => {
  authStore.rehydrateFromPersistedState()

  void (async () => {
    const remoteOverride = await AppDownloadService.loadOverride()
    applyOverrideToForm(remoteOverride)
    await loadReleases()
  })()
})
</script>

<style scoped>
.download-wrap {
  max-width: 1160px;
  margin: 0 auto;
  padding: 2rem 1rem 3rem;
  display: grid;
  gap: 1rem;
}

.hero-shell {
  border: 1px solid var(--wn-border);
  border-radius: 22px;
  background:
    radial-gradient(120% 130% at 0% 0%, color-mix(in srgb, var(--wn-primary) 17%, transparent), transparent 58%),
    radial-gradient(120% 130% at 100% 100%, color-mix(in srgb, var(--wn-link) 12%, transparent), transparent 58%),
    var(--wn-surface-soft);
  box-shadow: var(--wn-shadow-soft);
  padding: 1.25rem;
  display: grid;
  gap: 1rem;
  grid-template-columns: minmax(320px, 2fr) minmax(220px, 1fr);
}

.eyebrow {
  margin: 0 0 0.42rem;
  text-transform: uppercase;
  letter-spacing: 0.18em;
  font-size: 0.72rem;
  color: var(--wn-muted);
}

h1 {
  margin: 0;
  font-size: clamp(1.75rem, 3vw, 2.45rem);
}

.summary {
  margin: 0.82rem 0 0;
  color: var(--wn-muted);
  max-width: 72ch;
}

.hero-meta {
  display: grid;
  gap: 0.6rem;
}

.meta-item {
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  background: var(--wn-surface);
  padding: 0.55rem 0.72rem;
}

.meta-label {
  display: block;
  font-size: 0.75rem;
  color: var(--wn-muted);
  text-transform: uppercase;
  letter-spacing: 0.08em;
  margin-bottom: 0.18rem;
}

.loading-banner,
.error-banner {
  border-radius: 12px;
  padding: 0.6rem 0.82rem;
  margin: 0;
}

.success-banner {
  border-radius: 12px;
  padding: 0.6rem 0.82rem;
  margin: 0;
  border: 1px solid color-mix(in srgb, #22a06b 42%, var(--wn-border));
  color: #1b7e54;
  background: color-mix(in srgb, #22a06b 8%, var(--wn-surface));
}

.loading-banner {
  border: 1px solid var(--wn-border);
  color: var(--wn-muted);
  background: var(--wn-surface);
}

.error-banner {
  border: 1px solid color-mix(in srgb, #d64545 35%, var(--wn-border));
  color: #b83838;
  background: color-mix(in srgb, #d64545 7%, var(--wn-surface));
}

.workspace {
  display: grid;
  gap: 1rem;
  grid-template-columns: minmax(220px, 280px) minmax(0, 1fr);
}

.version-rail,
.release-panel,
.channel-rail,
.channel-panel,
.editor {
  border: 1px solid var(--wn-border);
  border-radius: 18px;
  background: var(--wn-surface);
  box-shadow: var(--wn-shadow-soft);
  padding: 1rem;
}

.channels {
  display: grid;
  gap: 1rem;
  grid-template-columns: minmax(220px, 280px) minmax(0, 1fr);
}

.version-rail h2,
.channel-rail h2,
.editor h2 {
  margin: 0 0 0.72rem;
}

.version-rail ul,
.channel-rail ul {
  margin: 0;
  padding: 0;
  list-style: none;
  display: grid;
  gap: 0.52rem;
}

.version-button,
.channel-button {
  width: 100%;
  text-align: left;
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  background: var(--wn-surface-soft);
  color: var(--wn-ink);
  padding: 0.55rem 0.68rem;
  display: grid;
  gap: 0.2rem;
}

.version-button.active,
.channel-button.active {
  border-color: var(--wn-primary);
  box-shadow: inset 0 0 0 1px color-mix(in srgb, var(--wn-primary) 45%, transparent);
}

.channel-rail-sub {
  margin: 0 0 0.72rem;
  color: var(--wn-muted);
}

.channels-cta-url {
  font-size: 0.82rem;
  color: var(--wn-link);
  overflow-wrap: anywhere;
}

.version-name {
  font-weight: 700;
}

.version-date,
.version-links {
  font-size: 0.82rem;
  color: var(--wn-muted);
}

.release-header {
  display: flex;
  flex-wrap: wrap;
  gap: 0.8rem;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.9rem;
}

.release-kicker {
  margin: 0;
  text-transform: uppercase;
  font-size: 0.72rem;
  letter-spacing: 0.14em;
  color: var(--wn-muted);
}

.release-header h2 {
  margin: 0.2rem 0;
}

.release-sub {
  margin: 0;
  color: var(--wn-muted);
}

.release-link {
  text-decoration: none;
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  padding: 0.52rem 0.78rem;
  color: var(--wn-ink);
  background: var(--wn-surface-soft);
}

.link-grid {
  display: grid;
  gap: 0.72rem;
  grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
}

.download-link-card {
  text-decoration: none;
  color: var(--wn-ink);
  border: 1px solid var(--wn-border);
  border-radius: 14px;
  background: var(--wn-surface-soft);
  padding: 0.7rem;
  display: grid;
  gap: 0.38rem;
}

.download-link-card:hover {
  border-color: color-mix(in srgb, var(--wn-primary) 45%, var(--wn-border));
}

.link-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.4rem;
}

.kind {
  font-size: 0.74rem;
  border-radius: 999px;
  padding: 0.2rem 0.48rem;
  border: 1px solid var(--wn-border);
  background: var(--wn-surface);
}

.kind[data-kind='installer'] {
  border-color: color-mix(in srgb, #22a06b 55%, var(--wn-border));
}

.kind[data-kind='archive'] {
  border-color: color-mix(in srgb, #3a89e5 55%, var(--wn-border));
}

.downloads,
.link-size {
  font-size: 0.8rem;
  color: var(--wn-muted);
}

.link-name {
  overflow-wrap: anywhere;
}

.hint,
.empty-state {
  margin: 0;
  color: var(--wn-muted);
}

.editor-grid {
  display: grid;
  gap: 0.62rem;
}

.editor-grid label {
  display: grid;
  gap: 0.26rem;
  font-size: 0.92rem;
}

.editor-grid input,
.editor-grid textarea,
.editor-grid select {
  border: 1px solid var(--wn-border);
  background: var(--wn-surface-soft);
  color: var(--wn-ink);
  border-radius: 10px;
  padding: 0.55rem 0.64rem;
}

.manual-section {
  border: 1px solid var(--wn-border);
  border-radius: 12px;
  padding: 0.72rem;
  display: grid;
  gap: 0.72rem;
  background: var(--wn-surface-soft);
}

.manual-section legend {
  padding: 0 0.3rem;
  color: var(--wn-muted);
  font-size: 0.86rem;
}

.manual-input-grid {
  display: grid;
  gap: 0.55rem;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
}

.btn-add {
  align-self: end;
  height: 2.35rem;
  border: 1px dashed var(--wn-primary);
  border-radius: 10px;
  color: var(--wn-primary);
  background: color-mix(in srgb, var(--wn-primary) 10%, transparent);
}

.manual-list {
  display: grid;
  gap: 0.62rem;
}

.manual-group {
  border: 1px solid var(--wn-border);
  border-radius: 10px;
  background: var(--wn-surface);
  padding: 0.62rem;
}

.manual-group header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.55rem;
}

.manual-group h3 {
  margin: 0;
  font-size: 1rem;
}

.manual-group header span {
  color: var(--wn-muted);
  font-size: 0.82rem;
}

.manual-group ul {
  margin: 0;
  padding: 0;
  list-style: none;
  display: grid;
  gap: 0.5rem;
}

.manual-group li {
  border: 1px solid var(--wn-border);
  border-radius: 10px;
  background: var(--wn-surface-soft);
  padding: 0.52rem;
  display: grid;
  gap: 0.5rem;
}

.manual-link-body {
  display: grid;
  gap: 0.24rem;
}

.manual-link-body a {
  color: var(--wn-link);
  text-decoration: none;
  overflow-wrap: anywhere;
  font-size: 0.84rem;
}

.btn-remove {
  justify-self: start;
  border: 1px solid color-mix(in srgb, #d64545 35%, var(--wn-border));
  border-radius: 8px;
  background: color-mix(in srgb, #d64545 8%, var(--wn-surface));
  color: #b83838;
  padding: 0.3rem 0.6rem;
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
  padding: 0.5rem 0.85rem;
  background: var(--wn-surface);
  color: var(--wn-ink);
}

.btn-save {
  border-color: var(--wn-primary);
  background: var(--wn-primary);
  color: var(--wn-on-primary);
}

@media (max-width: 980px) {
  .hero-shell,
  .channels,
  .workspace {
    grid-template-columns: 1fr;
  }
}
</style>
