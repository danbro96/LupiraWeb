import Link from "./Link";

export default function Footer() {
  return (
    <footer className="border-t border-slate-700 text-sm text-slate-400">
      <div className="max-w-5xl mx-auto px-6 py-4 space-y-2">
        <div className="flex  space-x-4 text-xs">
          <Link href="mailto:daniel.brostrom@hotmail.se">
            daniel.brostrom@hotmail.se
          </Link>
          <span>·</span>
          <Link href="tel:+46735028811">+46 (0)73 502 88 11</Link>
        </div>
        <div className="flex justify-between">
          <span>© Lupira.com</span>
          <Link href="/cookies">No cookies. No tracking.</Link>
        </div>
      </div>
    </footer>
  );
}
