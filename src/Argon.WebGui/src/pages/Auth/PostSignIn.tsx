import { useAuth } from "react-oidc-context";
import { Navigate } from "react-router-dom";

export default function PostSignIn() {
  const auth = useAuth();

  if (auth.isLoading) {
    return <div>Post sign in loading...</div>;
  }

  if (auth.error) {
    return <div>Post sign in error... {auth.error.message}</div>;
  }

  if (auth.isAuthenticated) {
    console.log("User is authenticated on post sign in");
    const state = JSON.parse(auth.user?.state as string) as {
      returnUrl?: string;
    };

    if (state.returnUrl) {
      console.debug("Redirecting to", state.returnUrl);
      return <Navigate to={state.returnUrl} />;
    }

    console.debug("Redirecting to dashboard");
    return <Navigate to="/dashboard" />;
  }

  return <div>Post sign in nothing to do!</div>;
}
