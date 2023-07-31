import { Outlet } from "react-router-dom";

export default function Layout() {
  return (
    <>
      <h1>Argon system layout</h1>
      <Outlet />
    </>
  );
}
