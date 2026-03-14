import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import { pinia } from '@/stores/pinia'

import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap'
import '@/assets/styles/site.scss'

import { CkeditorPlugin } from '@ckeditor/ckeditor5-vue'

const app = createApp(App)

app.use(pinia)
app.use(router)
app.use(CkeditorPlugin)
app.mount('#app')
