import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { createDiscreteApi } from 'naive-ui'
import router from './router'
import { i18n } from './i18n'
import { pinia } from './stores'
import { registerPermissionDirective } from './directives/permission'

const app = createApp(App)

app.use(pinia)
app.use(router)
app.use(i18n)

registerPermissionDirective(app)
createDiscreteApi(['message'])

app.mount('#app')
