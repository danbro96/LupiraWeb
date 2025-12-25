import ContactCard from "../components/ContactCard";

export default function About() {
  return (
    <article className="space-y-8">
      <header className="space-y-2">
        <h2 className="text-2xl font-semibold">About</h2>
        <p className="text-slate-400 max-w-3xl">
          Full-stack web developer with a mechanical engineering background and
          strong focus on systems thinking, automation, and reliable production
          environments.
        </p>
      </header>

      {/* Current role */}
      <section className="space-y-2">
        <h3 className="text-lg font-semibold text-gray-200">
          Current Role
        </h3>
        <p className="font-medium text-gray-200">
          Full-stack Web Developer at <span className="text-gray-200">Strivo AB</span>
        </p>

        <ul className="list-disc list-inside text-slate-400 space-y-1">
          <li>Main programming focus on .NET backend (controllers/minimal API)</li>
          <li>Frontend development in React, Blazor, and Razor Pages</li>
          <li>Responsible for company DevOps</li>
          <li>Migrating from on-premise servers to the cloud</li>
        </ul>
      </section>

      {/* Previous experience */}
      <section className="space-y-3">
        <h3 className="text-lg font-semibold text-gray-200">
          Previous Experience
        </h3>

        <div className="space-y-1">
          <p className="font-medium text-gray-200">
            Founder & CTO — RapAdd MFG AB
          </p>
          <p className="text-slate-400">
            Led R&amp;D and development of an automated 3D-printing manufacturing
            solution. Responsible for system architecture, full-stack web
            development, DevOps, and technical leadership of a small team.
          </p>
        </div>

        <div className="space-y-1">
          <p className="font-medium text-gray-200">
            Design Engineer / Production Technician — Firefly AB
          </p>
          <p className="text-slate-400">
            Mechanical and mechatronics development of fire-prevention equipment.
            Designed hardware, automation solutions, PLC systems, and internal
            tooling for production efficiency.
          </p>
        </div>
      </section>

      {/* Skills */}
      <section className="space-y-3">
        <h3 className="text-lg font-semibold text-gray-200">Core Skills</h3>

        <div className="grid gap-4 md:grid-cols-2 text-slate-400">
          <ul className="list-disc list-inside">
            <li>Frontend: React, Blazor, Razor Pages, TypeScript</li>
            <li>Backend: .NET (controllers/minimal API), Python, Java</li>
            <li>APIs & system design</li>
            <li>Git-based workflows</li>
          </ul>

          <ul className="list-disc list-inside">
            <li>Linux / FreeBSD environments</li>
            <li>AWS: EC2, RDS, S3, Amplify</li>
            <li>CI/CD & deployment automation</li>
            <li>Cloud migration & DevOps</li>
          </ul>
        </div>

        <div className="text-slate-400">
          <p className="mt-2">
            Hardware & engineering background:
          </p>
          <ul className="list-disc list-inside">
            <li>Mechanical CAD (SolidWorks, Inventor)</li>
            <li>PLC systems (Siemens TIA Portal, Codesys)</li>
            <li>PCB design (KiCad)</li>
            <li>Mechatronics and automation</li>
          </ul>
        </div>
      </section>

      {/* Education */}
      <section className="space-y-2">
        <h3 className="text-lg font-semibold text-gray-200">Education</h3>
        <p className="text-slate-400">
          Mechanical Engineering — KTH Royal Institute of Technology, Stockholm
          <br />
          Two years completed in the Master of Science program.
        </p>
      </section>

      <section className="space-y-2">
        <ContactCard />
      </section>

      {/* Resume download */}
      <section className="pt-4 border-t border-slate-700">
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
      </section>


    </article>
  );
}
