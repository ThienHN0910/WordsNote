<template>
  <header class="topbar glass-panel">
    <div class="brand">
      <RouterLink to="/quiz" class="brand-link">
        <span class="brand-code">FE Ôn tập</span>
      </RouterLink>
      <span class="brand-sub">{{ activeSubjectName }}</span>
    </div>

    <!-- Subject Tabs -->
    <nav class="subjects" aria-label="Chọn môn">
      <button
        v-for="s in subjects"
        :key="s.id"
        type="button"
        class="sub-tab"
        :class="{
          'active': activeSubjectId === s.id,
          'is-locked': s.isRestricted && !isUnlocked(s.id)
        }"
        :data-subject="s.id"
        :title="s.isRestricted && !isUnlocked(s.id) ? 'Cần mã mở khóa' : (s.name || s.title)"
        @click="$emit('select-subject', s.id)"
      >
        <span v-if="s.isRestricted && !isUnlocked(s.id)">🔒 </span>
        {{ s.code }}
      </button>
    </nav>

    <!-- Top Action Toolbar Buttons -->
    <div class="top-actions">
      <!-- Search Button -->
      <button
        type="button"
        class="btn ghost action-btn"
        title="Tìm kiếm câu hỏi"
        @click="$emit('open-search')"
      >
        <i class="fa-solid fa-magnifying-glass"></i>
      </button>

      <!-- Admin Button -->
      <button
        type="button"
        class="btn ghost action-btn admin-btn"
        :class="{ 'is-admin-highlight': user && user.isAdmin }"
        :title="user && user.isAdmin ? 'Quản lý Admin (Đã xác thực)' : 'Quản lý mã Admin'"
        @click="$emit('open-admin')"
      >
        <span v-if="user && user.isAdmin">👑 Admin</span>
        <span v-else>🔑 Admin</span>
      </button>

      <!-- Theme Toggle Button -->
      <button
        type="button"
        class="btn ghost action-btn"
        :title="isDark ? 'Giao diện Sáng' : 'Giao diện Tối'"
        @click="$emit('toggle-theme')"
      >
        <i :class="isDark ? 'fa-solid fa-sun' : 'fa-solid fa-moon'"></i>
      </button>

      <!-- User Profile / Login / Logout Button -->
      <div v-if="user" class="user-pill" title="Tài khoản học viên">
        <img v-if="user.picture" :src="user.picture" :alt="user.name" class="user-avatar" />
        <span v-else class="user-avatar-placeholder">{{ user.name.charAt(0).toUpperCase() }}</span>
        <span class="user-name">{{ user.name }}</span>
        <button type="button" class="btn ghost logout-btn" title="Đăng xuất" @click="$emit('logout')">
          <i class="fa-solid fa-right-from-bracket"></i>
        </button>
      </div>

      <button
        v-else
        type="button"
        class="btn ghost action-btn login-btn-compact"
        title="Đăng nhập Google"
        @click="$emit('open-login')"
      >
        <i class="fa-brands fa-google me-1"></i> Đăng nhập
      </button>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import type { QuizSet, QuizUser } from '@/types/WordsNote'

const props = defineProps<{
  subjects: QuizSet[]
  activeSubjectId: string
  user: QuizUser | null
  isDark: boolean
}>()

defineEmits<{
  (e: 'select-subject', subjectId: string): void
  (e: 'open-search'): void
  (e: 'open-admin'): void
  (e: 'open-login'): void
  (e: 'toggle-theme'): void
  (e: 'logout'): void
}>()

function isUnlocked(subjectId: string): boolean {
  if (!props.user) return false
  if (props.user.isAdmin) return true
  if (!props.user.unlockedSubjects) return false
  return props.user.unlockedSubjects.includes(subjectId.toLowerCase())
}

const activeSubjectName = computed(() => {
  const current = props.subjects.find((s) => s.id === props.activeSubjectId)
  if (!current) return '—'
  return `${current.code} · ${current.name || current.title}`
})
</script>

<style scoped>
.topbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 14px;
  padding: 10px 14px;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
}

.brand {
  display: flex;
  flex-direction: column;
  gap: 1px;
  min-width: 0;
}

.brand-link {
  text-decoration: none;
}

.brand-code {
  font-weight: 800;
  font-size: 1.05rem;
  background: linear-gradient(135deg, var(--wn-accent, #2563eb), #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.brand-sub {
  font-size: 0.75rem;
  color: var(--wn-ink-muted, #64748b);
  max-width: 260px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.subjects {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.sub-tab {
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  border-radius: 999px;
  padding: 6px 14px;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  color: var(--wn-ink-muted, #64748b);
  transition: all 0.15s ease;
}

.sub-tab:hover {
  border-color: #94a3b8;
  color: var(--wn-ink, #0f172a);
}

.sub-tab.active {
  background: #2563eb;
  border-color: #2563eb;
  color: #fff;
}

.sub-tab[data-subject="prm393"].active {
  background: #7c3aed;
  border-color: #7c3aed;
}

.sub-tab[data-subject="jfe301"].active {
  background: #059669;
  border-color: #059669;
}

.sub-tab[data-subject="jit401"].active {
  background: #dc2626;
  border-color: #dc2626;
}

.sub-tab.is-locked {
  opacity: 0.8;
  border-style: dashed;
}

.sub-tab.is-locked.active {
  opacity: 1;
  border-style: solid;
}

.top-actions {
  display: flex;
  align-items: center;
  gap: 6px;
}

.action-btn {
  background: transparent;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 8px;
  padding: 6px 10px;
  font-size: 0.82rem;
  color: var(--wn-ink, #0f172a);
  cursor: pointer;
  transition: all 0.15s ease;
}

.action-btn:hover {
  background: rgba(148, 163, 184, 0.12);
  border-color: #94a3b8;
}

.is-admin-highlight {
  background: rgba(245, 158, 11, 0.15) !important;
  border: 1px solid rgba(245, 158, 11, 0.5) !important;
  color: #d97706 !important;
  font-weight: 700;
}

.user-pill {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 8px;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 999px;
  margin-left: 4px;
}

.user-avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  object-fit: cover;
}

.user-avatar-placeholder {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #2563eb;
  color: #fff;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  font-weight: 700;
}

.user-name {
  font-size: 0.8rem;
  font-weight: 600;
  max-width: 110px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.logout-btn {
  background: transparent;
  border: none;
  padding: 2px 4px;
  font-size: 0.8rem;
  color: #ef4444;
  cursor: pointer;
}

.login-btn-compact {
  color: #2563eb;
  font-weight: 600;
}
</style>
