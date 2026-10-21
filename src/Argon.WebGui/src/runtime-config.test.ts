import { describe, expect, it } from "vitest";

import { resolveRuntimeConfig } from "./runtime-config";

const build = {
  VITE_APP_BACKEND_API_URI: "http://localhost:5000",
  VITE_APP_AUTHORITY: "https://development.example/",
  VITE_APP_CLIENT_ID: "local-client",
  VITE_APP_REDIRECT_URI: "http://localhost:5173/auth/post-sign-in",
  VITE_APP_SCOPE: "openid profile offline_access",
  VITE_APP_COMMIT_HASH: "immutable-source",
};

describe("deployment runtime configuration", () => {
  it("preserves local development and immutable source metadata", () => {
    expect(resolveRuntimeConfig(build)).toEqual(build);
  });

  it("promotes the same bundle with independent environment authentication", () => {
    const production = resolveRuntimeConfig(build, {
      VITE_APP_BACKEND_API_URI: "/api",
      VITE_APP_AUTHORITY: "https://production.example/",
      VITE_APP_CLIENT_ID: "production-client",
      VITE_APP_REDIRECT_URI: "https://production.example/auth/post-sign-in",
    });
    expect(production.VITE_APP_CLIENT_ID).toBe("production-client");
    expect(production.VITE_APP_BACKEND_API_URI).toBe("/api");
    expect(production.VITE_APP_COMMIT_HASH).toBe("immutable-source");
    expect(build.VITE_APP_CLIENT_ID).toBe("local-client");
  });
});
