import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    tailwindcss(),
    vue(),
  ],
  server: {
    proxy: {
      // Forward /api/* → backend (port 5000 trực tiếp, không qua nginx)
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      // Forward /ws/* → backend SignalR
      '/ws': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        ws: true,
      },
    }
  }
})
