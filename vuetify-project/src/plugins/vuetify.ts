/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Composables
import { createVuetify } from 'vuetify'
// Styles
import '@mdi/font/css/materialdesignicons.css'

import 'vuetify/styles'

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  display: {
    thresholds: {
      xs: 0,
      sm: 600,
      md: 960,
      lg: 1280, // Forces JavaScript to transition exactly at 1280px
      xl: 1920,
      xxl: 2560,
    },
  },
  theme: {
    defaultTheme: 'system',
  },
})
