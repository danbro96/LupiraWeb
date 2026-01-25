import React from "react";
import ContactCard from "../components/ContactCard";
import SectionCard from "../components/SectionCard";
import SkillBadge from "../components/SkillBadge";

interface Experience {
  title: string;
  company: string;
  description: string;
}

interface SkillCategory {
  title: string;
  skills: string[];
}

export default function About(): React.ReactElement {
  const experiences: Experience[] = [
    {
      title: "Founder & CTO",
      company: "RapAdd MFG AB",
      description: "Led R&D and development of an automated 3D-printing manufacturing platform. System architecture, full-stack development, DevOps, and technical leadership."
    },
    {
      title: "Design Engineer / Production Technician",
      company: "Firefly AB",
      description: "Mechanical and mechatronics development of fire-prevention systems. Hardware design, PLC automation, and production tooling."
    }
  ];

  const skillCategories: SkillCategory[] = [
    {
      title: "Software",
      skills: ["React", "Blazor", "Razor Pages", "TypeScript", ".NET", "Python", "Java", "UML design", "Git workflows"]
    },
    {
      title: "Infrastructure & DevOps",
      skills: ["Linux", "Windows Server", "AWS EC2/RDS/S3/Amplify", "Azure WebApp Services", "CI/CD", "Azure DevOps", "Yaml-pipelines"]
    },
    {
      title: "Hardware & Engineering",
      skills: ["SolidWorks", "Autodesk Inventor", "PLC (TIA Portal)", "Codesys", "KiCad", "CNC machining", "3D printing"]
    }
  ];

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
          <li>DevOps responsibility</li>
          <li>On-prem → cloud migration</li>
        </ul>
      </SectionCard>

      {/* Experience timeline */}
      <SectionCard title="Previous Experience">
        <div className="space-y-6">
          {experiences.map((exp, index) => (
            <div key={index}>
              <p className="font-medium text-gray-200">
                {exp.title} — {exp.company}
              </p>
              <p className="mt-1 text-slate-400">
                {exp.description}
              </p>
            </div>
          ))}
        </div>
      </SectionCard>

      {/* Skills */}
      <SectionCard title="Core Skills">
        <div className="space-y-6">
          {skillCategories.map((category, index) => (
            <div key={index}>
              <p className="mb-2 text-sm uppercase tracking-wide text-slate-500">
                {category.title}
              </p>
              <div className="flex flex-wrap gap-2">
                {category.skills.map((skill) => (
                  <SkillBadge key={skill} label={skill} />
                ))}
              </div>
            </div>
          ))}
        </div>
      </SectionCard>

      {/* Education */}
      <SectionCard title="Education">
        <p className="text-slate-400">
          Mechanical Engineering — KTH Royal Institute of Technology, Stockholm
          <br />
          Two years completed.
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
          Download resumé (PDF)
        </a>
      </div>
    </article>
  );
}
