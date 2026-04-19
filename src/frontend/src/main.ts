import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import { pinia } from '@/stores/pinia'
import { useAuthStore } from '@/stores/AS/AuthStore'

import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap'
import '@/assets/styles/site.scss'
import '@/assets/styles/app-identity.css'

const app = createApp(App)

app.use(pinia)

const authStore = useAuthStore(pinia)
authStore.rehydrateFromPersistedState()

app.use(router)
app.mount('#app')
