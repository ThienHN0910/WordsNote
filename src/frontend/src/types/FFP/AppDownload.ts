export interface GithubReleaseAsset {
  name: string
  browserDownloadUrl: string
  size: number
  downloadCount: number
  contentType: string
}

export type ReleaseDownloadKind = 'installer' | 'archive' | 'other'

export interface ReleaseDownloadLink {
  name: string
  url: string
  size: number
  downloadCount: number
  kind: ReleaseDownloadKind
}

export interface ManualReleaseDownloadLink {
  name: string
  url: string
  kind: ReleaseDownloadKind
}

export interface ManualReleaseLinksByVersion {
  tagName: string
  links: ManualReleaseDownloadLink[]
}

export interface GithubReleaseSnapshot {
  repo: string
  tagName: string
  name: string
  body: string
  publishedAt: string
  htmlUrl: string
  assets: GithubReleaseAsset[]
  fetchedAt: string
}

export interface DownloadPageOverride {
  title?: string
  summary?: string
  appStoreUrl?: string
  edgeAddonsUrl?: string
  repo?: string
  maxVisibleVersions?: number
  featuredTag?: string
  manualLinksByVersion?: ManualReleaseLinksByVersion[]
  updatedByEmail?: string
  updatedAt?: string
}
