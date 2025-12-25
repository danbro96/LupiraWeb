import Link from "./Link.tsx";

export default function Footer() {
  return (
    <footer className="border-t border-slate-700 text-sm text-slate-400">
      <div className="max-w-5xl mx-auto px-6 py-4 flex justify-between items-center">
        <span>© Lupira.com</span>
        <div className="text-xs space-x-4">
          <Link href="mailto:daniel.brostrom@hotmail.se">
            daniel.brostrom@hotmail.se
          </Link>
          <span>·</span>
          <Link href="tel:+46735028811">+46 735 028 811</Link>
        </div>
        <Link href="/cookies">No cookies. No tracking.</Link>
      </div>
    </footer>
  );
}
