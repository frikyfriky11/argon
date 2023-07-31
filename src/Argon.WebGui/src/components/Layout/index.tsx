import React, { useState } from "react";

import MainContent from "./MainContent";
import Navbar from "./Navbar";
import Sidebar from "./Sidebar";

export default function Index() {
  const [isSidebarOpen, setSidebarOpen] = useState(false);

  return (
    <>
      <MainContent />
      <Navbar
        onSidebarOpen={() => {
          setSidebarOpen(true);
        }}
      />
      <Sidebar
        onClose={() => {
          setSidebarOpen(false);
        }}
        open={isSidebarOpen}
      />
    </>
  );
}
