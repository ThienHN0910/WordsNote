import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig(({ mode }) => ({
  plugins: [vue()],
  
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },

  server: {
    port: 5173,
  },

  build: {
    sourcemap: mode === 'development',
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (!id.includes('node_modules')) return

          if (id.includes('ckeditor5') || id.includes('@ckeditor')) {
            return 'vendor-ckeditor'
          }

          if (id.includes('pdfjs-dist') || id.includes('pdf-lib')) {
            return 'vendor-pdf'
          }

          if (id.includes('vue') || id.includes('pinia') || id.includes('vue-router')) {
            return 'vendor-vue'
          }

          return 'vendor-misc'
        },
      },
    },
  },
}))
