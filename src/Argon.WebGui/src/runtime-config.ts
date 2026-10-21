type RuntimeConfig = {
  VITE_APP_BACKEND_API_URI: string;
  VITE_APP_RUNNING_ENVIRONMENT?: string;
  VITE_APP_BUILD_ID?: string;
  VITE_APP_COMMIT_HASH?: string;
  VITE_APP_AUTHORITY: string;
  VITE_APP_CLIENT_ID: string;
  VITE_APP_REDIRECT_URI: string;
  VITE_APP_SCOPE: string;
};

declare global {
  // Window must use an interface so TypeScript merges the browser declaration.
  // eslint-disable-next-line @typescript-eslint/consistent-type-definitions
  interface Window {
    ARGON_CONFIG?: Partial<RuntimeConfig>;
  }
}

export function resolveRuntimeConfig(
  build: RuntimeConfig,
  runtime: Partial<RuntimeConfig> = {},
): RuntimeConfig {
  return { ...build, ...runtime };
}

export const runtimeConfig = resolveRuntimeConfig(
  import.meta.env,
  typeof window === "undefined" ? {} : window.ARGON_CONFIG,
);
