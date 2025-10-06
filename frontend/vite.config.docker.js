
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: '0.0.0.0',
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://backend:5126',
        changeOrigin: true,
        secure: false,
      },
      '/ws': {
        target: 'ws://backend:5126',
        ws: true,
        changeOrigin: true,
      }
    }
  }
})
