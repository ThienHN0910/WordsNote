export interface GithubReleaseAsset {
  name: string
  browserDownloadUrl: string
  size: number
  downloadCount: number
  contentType: string
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
  primaryDownloadUrl?: string
  primaryAssetName?: string
  checksumUrl?: string
  releaseNotesUrl?: string
  versionLabel?: string
  publishedAt?: string
}
