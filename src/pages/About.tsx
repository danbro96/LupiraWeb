import ContactCard from "../components/ContactCard";

function SectionCard({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <section className="relative rounded-xl border border-slate-700 bg-slate-900/40 p-6">
      <div className="absolute left-0 top-0 h-full w-1 rounded-l-xl bg-teal-500/60" />
      <h3 className="mb-4 text-lg font-semibold text-gray-100">{title}</h3>
      {children}
    </section>
  );
}

function SkillBadge({ label }: { label: string }) {
  return (
    <span className="rounded-md border border-slate-600 px-2 py-1 text-sm text-slate-300">
      {label}
    </span>
  );
}

export default function About() {
  return (
    <article className="mx-auto max-w-5xl space-y-12">
      {/* Header */}
      <header className="space-y-4">
        <h2 className="text-3xl font-semibold tracking-tight text-gray-100">
          About
        </h2>
        <p className="max-w-3xl text-slate-400">
          Full-stack web developer with a mechanical engineering background and
          strong focus on systems thinking, automation, and reliable production
          environments.
        </p>
      </header>

      {/* Current role */}
      <SectionCard title="Current Role">
        <p className="font-medium text-gray-200">
          Full-stack Web Developer — Strivo AB
        </p>

        <ul className="mt-3 list-disc list-inside space-y-1 text-slate-400">
          <li>.NET backend (controllers & minimal APIs)</li>
          <li>React, Blazor, Razor Pages</li>
          <li>Company-wide DevOps responsibility</li>
          <li>On-prem → cloud migration</li>
        </ul>
      </SectionCard>

      {/* Experience timeline */}
      <SectionCard title="Previous Experience">
        <div className="space-y-6">
          <div>
            <p className="font-medium text-gray-200">
              Founder & CTO — RapAdd MFG AB
            </p>
            <p className="mt-1 text-slate-400">
              Led R&amp;D and development of an automated 3D-printing manufacturing
              platform. System architecture, full-stack development, DevOps, and
              technical leadership.
            </p>
          </div>

          <div>
            <p className="font-medium text-gray-200">
              Design Engineer / Production Technician — Firefly AB
            </p>
            <p className="mt-1 text-slate-400">
              Mechanical and mechatronics development of fire-prevention systems.
              Hardware design, PLC automation, and production tooling.
            </p>
          </div>
        </div>
      </SectionCard>

      {/* Skills */}
      <SectionCard title="Core Skills">
        <div className="space-y-6">
          <div>
            <p className="mb-2 text-sm uppercase tracking-wide text-slate-500">
              Software
            </p>
            <div className="flex flex-wrap gap-2">
              {[
                "React",
                "Blazor",
                "Razor Pages",
                "TypeScript",
                ".NET",
                "Python",
                "Java",
                "API design",
                "Git workflows",
              ].map((s) => (
                <SkillBadge key={s} label={s} />
              ))}
            </div>
          </div>

          <div>
            <p className="mb-2 text-sm uppercase tracking-wide text-slate-500">
              Infrastructure & DevOps
            </p>
            <div className="flex flex-wrap gap-2">
              {[
                "Linux",
                "FreeBSD",
                "AWS EC2",
                "RDS",
                "S3",
                "Amplify",
                "CI/CD",
                "Cloud migration",
              ].map((s) => (
                <SkillBadge key={s} label={s} />
              ))}
            </div>
          </div>

          <div>
            <p className="mb-2 text-sm uppercase tracking-wide text-slate-500">
              Hardware & Engineering
            </p>
            <div className="flex flex-wrap gap-2">
              {[
                "SolidWorks",
                "Inventor",
                "PLC (TIA Portal)",
                "Codesys",
                "KiCad",
                "Mechatronics",
                "Automation",
              ].map((s) => (
                <SkillBadge key={s} label={s} />
              ))}
            </div>
          </div>
        </div>
      </SectionCard>

      {/* Education */}
      <SectionCard title="Education">
        <p className="text-slate-400">
          Mechanical Engineering — KTH Royal Institute of Technology, Stockholm
          <br />
          Two years completed in the Master of Science program.
        </p>
      </SectionCard>

      {/* Contact */}
      <SectionCard title="Contact">
        <ContactCard />
      </SectionCard>

      {/* Resume */}
      <div className="flex justify-end border-t border-slate-800 pt-6">
        <a
          href="/Resume_Daniel_Brostrom.pdf"
          download
          className="
            inline-flex items-center gap-2
            rounded-md border border-slate-600
            px-4 py-2 text-sm
            text-gray-200
            hover:border-teal-400 hover:text-teal-400
            transition-colors
          "
        >
          Download résumé (PDF)
        </a>
      </div>
    </article>
  );
}
