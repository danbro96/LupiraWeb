import { useEffect } from "react";
import { Outlet, useLocation } from "react-router-dom";
import Topbar from "./Topbar.tsx";
import Footer from "./Footer.tsx";

export default function Layout() {
  const location = useLocation();

  useEffect(() => {
    const path = location.pathname;
    if (path === "/") {
      document.title = "Lupira";
    } else if (path === "/about") {
      document.title = "About - Lupira";
    } else if (path === "/projects") {
      document.title = "Projects - Lupira";
    } else if (path === "/projects/example") {
      document.title = "Project Example - Lupira";
    } else if (path === "/kokos") {
      document.title = "Kokos - Lupira";
    } else if (path === "/cookies") {
      document.title = "Cookies - Lupira";
    } else {
      document.title = "Lupira";
    }
  }, [location.pathname]);

  return (
    <div className="min-h-screen bg-slate-900 text-gray-200 flex flex-col">
      <Topbar />
      <main className="flex-1 max-w-5xl mx-auto px-6 py-10">
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}
