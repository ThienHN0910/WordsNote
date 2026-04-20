<template>
  <section class="catalog-shell">
    <h2>All GitHub versions and assets</h2>
    <div class="catalog-grid">
      <article class="catalog-card" v-for="item in releaseCards" :key="item.release.tagName || item.release.name">
        <header>
          <h3>{{ item.release.tagName || item.release.name }}</h3>
          <p>{{ formatPublishedAt(item.release.publishedAt) }}</p>
        </header>

        <ul>
          <li v-for="link in item.links" :key="link.url">
            <a :href="link.url" target="_blank" rel="noopener noreferrer">{{ link.name }}</a>
            <span>{{ formatFileSize(link.size) }}</span>
          </li>
        </ul>
      </article>
    </div>
  </section>
</template>

<script setup lang="ts">
import type { GithubReleaseSnapshot, ReleaseDownloadLink } from '@/types/FFP/AppDownload'

interface ReleaseCard {
  release: GithubReleaseSnapshot
  links: ReleaseDownloadLink[]
}

defineProps<{
  releaseCards: ReleaseCard[]
}>()

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
</script>

<style scoped>
.catalog-shell {
  border: 1px solid var(--wn-border);
  border-radius: 18px;
  background: var(--wn-surface);
  box-shadow: var(--wn-shadow-soft);
  padding: 1rem;
}

.catalog-shell h2 {
  margin: 0 0 0.72rem;
}

.catalog-grid {
  display: grid;
  gap: 0.8rem;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
}

.catalog-card {
  border: 1px solid var(--wn-border);
  border-radius: 14px;
  padding: 0.75rem;
  background: var(--wn-surface-soft);
}

.catalog-card h3 {
  margin: 0;
}

.catalog-card p {
  margin: 0.2rem 0 0.6rem;
  color: var(--wn-muted);
  font-size: 0.86rem;
}

.catalog-card ul {
  margin: 0;
  padding: 0;
  list-style: none;
  display: grid;
  gap: 0.36rem;
}

.catalog-card li {
  display: flex;
  justify-content: space-between;
  gap: 0.5rem;
  font-size: 0.9rem;
}

.catalog-card a {
  color: var(--wn-link);
  text-decoration: none;
  overflow-wrap: anywhere;
}
</style>
