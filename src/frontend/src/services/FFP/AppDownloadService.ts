import type {
  DownloadPageOverride,
  GithubReleaseAsset,
  GithubReleaseSnapshot,
  ManualReleaseDownloadLink,
  ManualReleaseLinksByVersion,
  ReleaseDownloadKind,
  ReleaseDownloadLink,
} from '@/types/FFP/AppDownload'
import { DownloadConfigAPI } from '@/apis/FFP/DownloadConfigAPI'

export const AppDownloadService = {
  async fetchReleases(repo: string, perPage = 20) {
    const normalizedRepo = normalizeRepo(repo)
    const response = await fetch(
      `https://api.github.com/repos/${normalizedRepo}/releases?per_page=${normalizePerPage(perPage)}`,
      {
      headers: {
        Accept: 'application/vnd.github+json',
      },
      },
    )

    if (!response.ok) {
      throw new Error(`GitHub release request failed: ${response.status}`)
    }

    const payload = (await response.json()) as unknown
    const releaseRows = Array.isArray(payload) ? payload : []

    return releaseRows
      .map((row) => mapReleaseSnapshot(row, normalizedRepo))
      .filter((row) => Boolean(row.tagName || row.name))
  },

  buildReleaseLinks(assets: GithubReleaseAsset[], manualLinks: ManualReleaseDownloadLink[] = []) {
    const linksFromGithub: ReleaseDownloadLink[] = assets
      .filter((asset) => isDownloadAsset(asset.name))
      .map((asset) => ({
        name: asset.name,
        url: asset.browserDownloadUrl,
        size: asset.size,
        downloadCount: asset.downloadCount,
        kind: classifyAssetKind(asset.name),
      }))

    const linksFromManual: ReleaseDownloadLink[] = manualLinks
      .map((link) => ({
        name: normalizeText(link.name),
        url: normalizeText(link.url),
        size: 0,
        downloadCount: 0,
        kind: normalizeKind(link.kind),
      }))
      .filter((link) => Boolean(link.name && link.url))

    const links = [...linksFromGithub, ...linksFromManual]

    return links.sort((a, b) => {
      const byKind = getKindPriority(a.kind) - getKindPriority(b.kind)
      if (byKind !== 0) {
        return byKind
      }

      return b.downloadCount - a.downloadCount
    })
  },

  getManualLinksForTag(override: DownloadPageOverride, tagName: string) {
    const normalizedTag = normalizeText(tagName).toLowerCase()
    if (!normalizedTag) {
      return [] as ManualReleaseDownloadLink[]
    }

    const versions = sanitizeManualLinksByVersion(override.manualLinksByVersion)
    const matched = versions.find((version) => normalizeText(version.tagName).toLowerCase() === normalizedTag)
    return matched?.links || []
  },

  async loadOverride() {
    try {
      const response = await DownloadConfigAPI.getConfig()
      return sanitizeOverride(response.data || ({} as DownloadPageOverride))
    } catch {
      return {} as DownloadPageOverride
    }
  },

  async saveOverride(override: DownloadPageOverride) {
    const payload = sanitizeOverride(override)
    const response = await DownloadConfigAPI.upsertConfig(payload)
    return sanitizeOverride(response.data || payload)
  },

  async clearOverride() {
    await DownloadConfigAPI.resetConfig()
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

function mapReleaseSnapshot(rawRelease: unknown, repo: string): GithubReleaseSnapshot {
  const release = (rawRelease ?? {}) as Record<string, unknown>
  const assetsRaw = Array.isArray(release.assets) ? release.assets : []

  return {
    repo,
    tagName: String(release.tag_name ?? '').trim(),
    name: String(release.name ?? '').trim(),
    body: String(release.body ?? ''),
    publishedAt: String(release.published_at ?? ''),
    htmlUrl: String(release.html_url ?? '').trim(),
    assets: assetsRaw.map(mapReleaseAsset).filter((asset) => Boolean(asset.browserDownloadUrl)),
    fetchedAt: new Date().toISOString(),
  }
}

function normalizePerPage(perPage: number) {
  if (!Number.isFinite(perPage)) {
    return 20
  }

  return Math.min(30, Math.max(1, Math.trunc(perPage)))
}

function isDownloadAsset(assetName: string) {
  const normalized = assetName.trim().toLowerCase()
  if (!normalized) {
    return false
  }

  if (normalized.endsWith('.appxsym') || normalized.endsWith('.msixupload') || normalized.endsWith('.appxupload')) {
    return false
  }

  return normalized.endsWith('.msixbundle')
    || normalized.endsWith('.msix')
    || normalized.endsWith('.exe')
    || normalized.endsWith('.msi')
    || normalized.endsWith('.zip')
}

function classifyAssetKind(assetName: string): ReleaseDownloadKind {
  const normalized = assetName.trim().toLowerCase()

  if (normalized.endsWith('.msixbundle') || normalized.endsWith('.msix') || normalized.endsWith('.exe') || normalized.endsWith('.msi')) {
    return 'installer'
  }

  if (normalized.endsWith('.zip')) {
    return 'archive'
  }

  return 'other'
}

function normalizeKind(kind: unknown): ReleaseDownloadKind {
  const normalized = String(kind || '').trim().toLowerCase()
  if (normalized === 'installer') {
    return 'installer'
  }

  if (normalized === 'archive') {
    return 'archive'
  }

  return 'other'
}

function getKindPriority(kind: ReleaseDownloadKind) {
  switch (kind) {
    case 'installer':
      return 0
    case 'archive':
      return 1
    default:
      return 2
  }
}

function sanitizeOverride(override: DownloadPageOverride) {
  return {
    title: normalizeText(override.title),
    summary: normalizeText(override.summary),
    repo: normalizeText(override.repo),
    maxVisibleVersions: normalizeCount(override.maxVisibleVersions),
    featuredTag: normalizeText(override.featuredTag),
    manualLinksByVersion: sanitizeManualLinksByVersion(override.manualLinksByVersion),
    updatedByEmail: normalizeText(override.updatedByEmail),
    updatedAt: normalizeText(override.updatedAt),
  } as DownloadPageOverride
}

function sanitizeManualLinksByVersion(rawVersions: unknown) {
  const rows = Array.isArray(rawVersions) ? rawVersions : []

  return rows
    .map((rawVersion) => {
      const version = (rawVersion || {}) as ManualReleaseLinksByVersion
      const links = Array.isArray(version.links) ? version.links : []

      return {
        tagName: normalizeText(version.tagName),
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
    .filter((version) => Boolean(version.tagName && version.links.length > 0))
}

function normalizeText(value: unknown) {
  return typeof value === 'string' ? value.trim() : ''
}

function normalizeCount(value: unknown) {
  const numeric = Number(value)
  if (!Number.isFinite(numeric)) {
    return 8
  }

  return Math.min(30, Math.max(1, Math.trunc(numeric)))
}
