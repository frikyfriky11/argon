import { Outlet } from "react-router-dom";

export default function Layout() {
  return (
    <>
      <h1>Auth layout</h1>
      <Outlet />
    </>
  );
}
