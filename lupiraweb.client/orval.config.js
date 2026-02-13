import { defineConfig } from 'orval';

export default defineConfig({
    backendApi: {
        output: {
            mode: 'single',
            target: './src/api/lupiraWebServerApi.ts',
            schemas: './src/api/models',
            mock: true,
        },
        input: {
            target: './backend-openapi.json',
        },
    },
});