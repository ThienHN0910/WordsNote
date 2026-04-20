import type { DownloadPageOverride, GithubReleaseAsset, GithubReleaseSnapshot } from '@/types/FFP/AppDownload'

const DOWNLOAD_OVERRIDE_STORAGE_KEY = 'wordsnote-download-page-override-v1'

export const AppDownloadService = {
  async fetchLatestRelease(repo: string) {
    const normalizedRepo = normalizeRepo(repo)
    const response = await fetch(`https://api.github.com/repos/${normalizedRepo}/releases/latest`, {
      headers: {
        Accept: 'application/vnd.github+json',
      },
    })

    if (!response.ok) {
      throw new Error(`GitHub release request failed: ${response.status}`)
    }

    const payload = (await response.json()) as Record<string, unknown>
    const assetsRaw = Array.isArray(payload.assets) ? payload.assets : []
    const assets = assetsRaw.map(mapReleaseAsset).filter((asset) => Boolean(asset.browserDownloadUrl))

    const snapshot: GithubReleaseSnapshot = {
      repo: normalizedRepo,
      tagName: String(payload.tag_name ?? '').trim(),
      name: String(payload.name ?? '').trim(),
      body: String(payload.body ?? ''),
      publishedAt: String(payload.published_at ?? ''),
      htmlUrl: String(payload.html_url ?? ''),
      assets,
      fetchedAt: new Date().toISOString(),
    }

    return snapshot
  },

  findPreferredInstallerAsset(assets: GithubReleaseAsset[]) {
    const byPriority = [
      (asset: GithubReleaseAsset) => asset.name.toLowerCase().endsWith('.msixbundle'),
      (asset: GithubReleaseAsset) => asset.name.toLowerCase().endsWith('.msix'),
      (asset: GithubReleaseAsset) => asset.name.toLowerCase().endsWith('.exe'),
      (asset: GithubReleaseAsset) => asset.name.toLowerCase().endsWith('.zip'),
    ]

    for (const matcher of byPriority) {
      const matched = assets.find(matcher)
      if (matched) {
        return matched
      }
    }

    return assets[0]
  },

  loadOverride() {
    if (typeof window === 'undefined') {
      return {} as DownloadPageOverride
    }

    const raw = window.localStorage.getItem(DOWNLOAD_OVERRIDE_STORAGE_KEY)
    if (!raw) {
      return {} as DownloadPageOverride
    }

    try {
      const parsed = JSON.parse(raw) as DownloadPageOverride
      return sanitizeOverride(parsed)
    } catch {
      return {} as DownloadPageOverride
    }
  },

  saveOverride(override: DownloadPageOverride) {
    if (typeof window === 'undefined') {
      return
    }

    window.localStorage.setItem(DOWNLOAD_OVERRIDE_STORAGE_KEY, JSON.stringify(sanitizeOverride(override)))
  },

  clearOverride() {
    if (typeof window === 'undefined') {
      return
    }

    window.localStorage.removeItem(DOWNLOAD_OVERRIDE_STORAGE_KEY)
  },
}

function normalizeRepo(repo: string) {
  const trimmed = repo.trim().replace(/^https?:\/\/github\.com\//i, '').replace(/\.git$/i, '')
  if (trimmed.includes('/')) {
    const [owner, name] = trimmed.split('/').map((part) => part.trim()).filter(Boolean)
    if (owner && name) {
      return `${owner}/${name}`
    }
  }

  return 'ThinHN/WordsNote'
}

function mapReleaseAsset(rawAsset: unknown): GithubReleaseAsset {
  const asset = (rawAsset ?? {}) as Record<string, unknown>

  return {
    name: String(asset.name ?? '').trim(),
    browserDownloadUrl: String(asset.browser_download_url ?? '').trim(),
    size: Number(asset.size ?? 0),
    downloadCount: Number(asset.download_count ?? 0),
    contentType: String(asset.content_type ?? '').trim(),
  }
}

function sanitizeOverride(override: DownloadPageOverride) {
  return {
    title: normalizeText(override.title),
    summary: normalizeText(override.summary),
    primaryDownloadUrl: normalizeText(override.primaryDownloadUrl),
    primaryAssetName: normalizeText(override.primaryAssetName),
    checksumUrl: normalizeText(override.checksumUrl),
    releaseNotesUrl: normalizeText(override.releaseNotesUrl),
    versionLabel: normalizeText(override.versionLabel),
    publishedAt: normalizeText(override.publishedAt),
  } as DownloadPageOverride
}

function normalizeText(value: unknown) {
  return typeof value === 'string' ? value.trim() : ''
}
