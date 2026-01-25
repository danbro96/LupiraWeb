import Link from "../components/Link";

export default function NotFound(): React.ReactElement {
  return (
    <section className="flex flex-col items-center justify-center min-h-[50vh] text-center space-y-4">
      <h1 className="text-4xl font-bold text-slate-200">404</h1>
      <p className="text-slate-400">Page not found</p>
      <Link href="/">Go back home</Link>
    </section>
  );
}
