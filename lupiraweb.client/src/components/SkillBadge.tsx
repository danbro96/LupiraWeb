export type SkillBadgeProps = {
  label: string;
};

export default function SkillBadge({
  label,
}: SkillBadgeProps): React.ReactElement {
  return (
    <span className="rounded-md border border-slate-600 px-2 py-1 text-sm text-slate-300">
      {label}
    </span>
  );
}
