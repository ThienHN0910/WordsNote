import { defineStore } from 'pinia'
import { useDark } from '@vueuse/core'
import { watch } from 'vue'

export const useThemeStore = defineStore('theme', () => {
  const isDark = useDark({
    storageKey: 'theme',
    valueDark: 'dark',
    valueLight: 'light',
    attribute: 'data-theme',
    selector: 'body'
  })

  watch(
    isDark,
    (val) => {
      if (typeof document === 'undefined') return
      const mode = val ? 'dark' : 'light'
      document.documentElement.setAttribute('data-theme', mode)
      document.documentElement.setAttribute('data-bs-theme', mode)
      document.body.setAttribute('data-theme', mode)
      document.body.setAttribute('data-bs-theme', mode)
      if (val) {
        document.documentElement.classList.add('dark')
        document.body.classList.add('dark')
      } else {
        document.documentElement.classList.remove('dark')
        document.body.classList.remove('dark')
      }
    },
    { immediate: true }
  )

  return { isDark }
})