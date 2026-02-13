import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// When running inside docker-compose we set VITE_DEV_API to e.g. "http://backend:80"
const devApiTarget = process.env.VITE_DEV_API || "http://localhost:5188";

export default defineConfig({
    plugins: [react()],
    server: {
        host: true,
        port: 5173,
        proxy: {
            // Proxy all /api requests to the backend without rewriting the path.
            "/api": {
                target: devApiTarget,
                changeOrigin: true,
                secure: false
            }
        }
    }
});