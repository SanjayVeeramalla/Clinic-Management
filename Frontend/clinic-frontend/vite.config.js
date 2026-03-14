import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],

  server: {
    port: 3000,       // same port as before so nothing else needs updating
    open: true,       // auto-open browser on npm run dev

    // Proxy /api/* requests to the .NET backend during development.
    // This avoids CORS issues — the browser only talks to localhost:3000.
    proxy: {
      '/api': {
        target:       'http://localhost:5000',
        changeOrigin: true,
        secure:       false,
      },
    },
  },

  build: {
    outDir:      'dist',
    sourcemap:   true,
  },
});