import { useEffect, useState } from "react";
import { hasAuthParams, useAuth } from "react-oidc-context";
import { useSearchParams } from "react-router-dom";

export default function SignIn() {
  const auth = useAuth();
  const [hasTriedSignin, setHasTriedSignin] = useState(false);
  const [searchParams] = useSearchParams();
  const returnUrl = searchParams.get("returnUrl");

  useEffect(() => {
    if (
      !hasAuthParams() &&
      !auth.isAuthenticated &&
      !auth.activeNavigator &&
      !auth.isLoading &&
      !hasTriedSignin
    ) {
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
    return <button onClick={() => void auth.removeUser()}>Log out</button>;
  }

  return <button onClick={() => void auth.signinRedirect()}>Log in</button>;
}
