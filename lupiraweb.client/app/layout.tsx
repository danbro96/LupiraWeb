import type { Metadata, Viewport } from "next";
import Topbar from "@/src/components/Topbar";
import Footer from "@/src/components/Footer";
import "./globals.css";

export const metadata: Metadata = {
  title: {
    template: "%s - Lupira",
    default: "Lupira",
  },
};

// The mark is served from app/icon.svg by the App Router file convention, which emits the
// <link rel="icon"> this page previously lacked — browsers were falling back to /favicon.ico.
export const viewport: Viewport = {
  themeColor: "#E76F51",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>
        <div className="min-h-screen bg-slate-900 text-gray-200 flex flex-col">
          <Topbar />
          <main className="flex-1 max-w-5xl mx-auto px-6 py-10">
            {children}
          </main>
          <Footer />
        </div>
      </body>
    </html>
  );
}
