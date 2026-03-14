import { defineStore } from 'pinia'
import { useDark } from '@vueuse/core'

export const useThemeStore = defineStore('theme', () => {
  const isDark = useDark({
	storageKey: 'theme',
	valueDark: 'dark',
	valueLight: 'light',
	attribute: 'data-theme',
	selector: 'body'
  })

  return { isDark }
})