import { CssBaseline, ThemeProvider } from "@mui/material";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ConfirmProvider } from "material-ui-confirm";
import { SnackbarProvider } from "notistack";
import { WebStorageStateStore } from "oidc-client-ts";
import React, { Suspense } from "react";
import ReactDOM from "react-dom/client";
import { AuthProvider, AuthProviderProps } from "react-oidc-context";

import { theme } from "./config/MaterialTheme";
import MainRouter from "./router";
import "./utils/TranslationSetup";

console.log("Vite environment:", import.meta.env.MODE);
console.log("Environment:", import.meta.env.VITE_APP_RUNNING_ENVIRONMENT);
console.log("Build:", import.meta.env.VITE_APP_BUILD_ID);
console.log("Commit:", import.meta.env.VITE_APP_COMMIT_HASH);

const oidcConfig: AuthProviderProps = {
  authority: import.meta.env.VITE_APP_AUTHORITY,
  client_id: import.meta.env.VITE_APP_CLIENT_ID,
  redirect_uri: import.meta.env.VITE_APP_REDIRECT_URI,
  scope: import.meta.env.VITE_APP_SCOPE,
  userStore: new WebStorageStateStore({ store: window.localStorage }),
};

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById("root")!).render(
  <Suspense fallback="Application is loading...">
    {/* this adds the authentication logic for OAuth 2 */}
    <AuthProvider {...oidcConfig}>
      {/* this creates a theme context that every MaterialUI component can use */}
      <ThemeProvider theme={theme}>
        {/* this resets the CSS to the defaults of the MaterialUI */}
        <CssBaseline />
        {/* this adds the possibility to use Notistack's snackbars throughout the entire application */}
        <SnackbarProvider>
          {/* this adds the possibility to use Material UI confirmation dialogs throughout the entire application */}
          <ConfirmProvider>
            <QueryClientProvider client={queryClient}>
              {/* this component configures the routes of the application */}
              <MainRouter />
            </QueryClientProvider>
          </ConfirmProvider>
        </SnackbarProvider>
      </ThemeProvider>
    </AuthProvider>
  </Suspense>,
);
