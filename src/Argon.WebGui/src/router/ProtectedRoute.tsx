import { useAuth } from "react-oidc-context";
import { Navigate, Outlet, useLocation } from "react-router-dom";

export default function ProtectedRoute() {
  const auth = useAuth();
  const { pathname, search } = useLocation();
  const returnUrl = pathname + search;

  console.debug("Route", returnUrl, "is protected");

  if (auth.isAuthenticated) {
    console.debug("User is authenticated, allowing route");
    return <Outlet />;
  }

  console.log("User is not authenticated, redirecting to sign in");
  return <Navigate to={`/auth/sign-in?returnUrl=${returnUrl}`} />;
}
