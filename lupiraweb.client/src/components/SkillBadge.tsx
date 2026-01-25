export default function SkillBadge({ label }: { label: string; }) {
  return (
    <span className="rounded-md border border-slate-600 px-2 py-1 text-sm text-slate-300">
      {label}
    </span>
  );
}
