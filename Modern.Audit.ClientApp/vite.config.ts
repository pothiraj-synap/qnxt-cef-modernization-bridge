import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: true, // Allows the container network to expose the port cleanly
    port: 3000, // Locks the frontend runtime server to port 3000
    strictPort: true,
    proxy: {
      // Correctly routes client frontend graph updates directly to the C# Web API server
      '/graphql': {
        target: 'http://localhost:5141',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
