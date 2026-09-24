# 0002. Single-Page Application (SPA) SEO and Metadata Strategy

Date: 2026-09-24

## Context and Problem Statement
WordsNote operates as a Vue 3 Single-Page Application (SPA) deployed at `https://words-note.thienhn.io.vn/`. Client-side SPAs face indexing and social card sharing challenges:
1. Social media crawlers (Facebook, Zalo, Twitter/X, Discord) and standard search engine bots cannot execute complex client-side JavaScript to read metadata that updates after mount.
2. Search crawlers index generic fallback titles unless dynamic route metadata, structured data (JSON-LD), `sitemap.xml`, and `robots.txt` are explicitly established.
3. Private study areas (`/manage`, `/manage/:deckId/session`) and authentication flows (`/login`, `/register`, `/logout`) must not be indexed to prevent duplicate or low-value content indexing.

## Decision
1. **Dynamic Navigation-Guarded Meta Injection (`seo.ts`)**:
   - Provide a lightweight, zero-dependency SEO utility `src/frontend/src/utils/seo.ts` executed on Vue Router navigation (`router.afterEach`).
   - Automatically synchronizes document title, meta description, canonical URL, OpenGraph tags (`og:*`), Twitter Cards (`twitter:*`), and `robots` directive (`index, follow` vs `noindex, nofollow`).
2. **Schema.org Structured Data (JSON-LD)**:
   - Inject a dynamic `<script type="application/ld+json">` tag managed by the router.
   - Root page emits `WebApplication` and `BreadcrumbList`.
   - Quiz Set pages (`/quiz/:id`) emit `Course` and `Quiz` / `LearningResource` schemas with subject metadata.
3. **Static Crawler Gateways (`robots.txt` and `sitemap.xml`)**:
   - Place a standard `robots.txt` in `public/` allowing crawler access to discovery routes (`/`, `/quiz`, `/quiz/:id`, `/download`, `/privacy-policy`) and explicitly disallowing `/manage`, `/manage/*`, `/login`, `/register`, `/logout`, and `/api/*`.
   - Place `sitemap.xml` in `public/` referencing canonical URLs for all public landing, download, policy, and educational Quiz Set endpoints (`mln122`, `prm393`, `jfe301`, `jit401`).
4. **Dedicated Social Preview Asset**:
   - Deliver high-resolution OpenGraph branding image (`public/images/og-preview.png`, 1200x630) ensuring consistent link embeds across social networks.

## Consequences
- **Positive**: Immediate boost to search discoverability and rich snippet eligibility without complex SSR/SSG server infrastructure.
- **Negative / Trade-off**: Previews for dynamic subjects in crawlers that strictly do not execute JS will receive the primary site-level OpenGraph tags embedded in `index.html`. Full pre-rendering (SSG) is deferred until question-level deep URL indexing is mandated.
