import { useEffect, useState } from "react";
import { hasAuthParams, useAuth } from "react-oidc-context";
import { Outlet, useLocation } from "react-router-dom";

export default function ProtectedRoute() {
  const auth = useAuth();
  const [hasTriedSignin, setHasTriedSignin] = useState(false);
  const { pathname, search } = useLocation();
  const returnUrl = pathname + search;

  console.debug("Route", returnUrl, "is protected");

  useEffect(() => {
    if (
      !hasAuthParams() &&
      !auth.isAuthenticated &&
      !auth.activeNavigator &&
      !auth.isLoading &&
      !hasTriedSignin
    ) {
      console.log("User is not authenticated, redirecting to sign in");
      void auth.signinRedirect({ state: JSON.stringify({ returnUrl }) });
      setHasTriedSignin(true);
    }
  }, [auth, hasTriedSignin, returnUrl]);

  if (auth.isLoading) {
    return <div>Auth loading...</div>;
  }

  if (auth.error) {
    return <div>Auth error... {auth.error.message}</div>;
  }

  if (auth.isAuthenticated) {
    console.debug("User is authenticated, allowing route");
    return <Outlet />;
  }

  return (
    <button
      onClick={() =>
        void auth.signinRedirect({ state: JSON.stringify({ returnUrl }) })
      }
    >
      Log in
    </button>
  );
}
