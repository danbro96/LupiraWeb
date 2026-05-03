import Link from "./Link";

export default function Topbar(): React.ReactElement {
  return (
    <header className="border-b border-slate-700">
      <div className="max-w-5xl mx-auto flex items-center gap-6 px-6 py-4">
        <Link href="/" aria-label="Back to homepage">
          <img src="/logo.svg" alt="Lupira" className="w-8 h-8" />
        </Link>
        <nav className="flex gap-4 text-sm">
          <Link href="/">Home</Link>
          <Link href="/about">About</Link>
          {/* <Link href="/projects">Projects</Link> */}
          <Link href="/kokos">Kokos</Link>
          <Link href="/demos">Demos</Link>
        </nav>
      </div>
    </header>
  );
}
