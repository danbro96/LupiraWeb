export type SectionCardProps = {
  title: string;
  children: React.ReactNode;
};

export default function SectionCard({
  title,
  children,
}: SectionCardProps): React.ReactElement {
  return (
    <section className="relative rounded-xl border border-slate-700 bg-slate-900/40 p-6">
      <div className="absolute left-0 top-0 h-full w-1 rounded-l-xl bg-teal-500/60" />
      <h3 className="mb-4 text-lg font-semibold text-gray-100">{title}</h3>
      {children}
    </section>
  );
}
