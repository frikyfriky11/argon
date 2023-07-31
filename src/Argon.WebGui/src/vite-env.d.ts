/// <reference types="vite/client" />

/**
 * See https://vitejs.dev/guide/env-and-mode.html#intellisense-for-typescript for more details
 */
// eslint-disable-next-line @typescript-eslint/consistent-type-definitions
interface ImportMetaEnv {
  readonly VITE_APP_BACKEND_API_URI: string;
  readonly VITE_APP_RUNNING_ENVIRONMENT?: string;
  readonly VITE_APP_BUILD_ID?: string;
  readonly VITE_APP_COMMIT_HASH?: string;
}

// eslint-disable-next-line @typescript-eslint/consistent-type-definitions
interface ImportMeta {
  readonly env: ImportMetaEnv;
}
