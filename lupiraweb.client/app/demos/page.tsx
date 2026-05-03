import type { Metadata } from "next";
import Link from "@/src/components/Link";
import SectionCard from "@/src/components/SectionCard";

export const metadata: Metadata = {
  title: "Demos",
};

interface Demo {
  href: string;
  title: string;
  description: string;
}

const demos: Demo[] = [
  {
    href: "/demos/chat",
    title: "Chat",
    description:
      "Send a prompt to a self-hosted LLM and read the reply. Single-shot, no history.",
  },
  {
    href: "/demos/text-to-speech",
    title: "Text-to-Speech",
    description:
      "Type some text, pick a voice and speed, and play the synthesized audio.",
  },
  {
    href: "/demos/vision",
    title: "Vision",
    description:
      "Upload an image and run a caption, OCR, or object-detection task.",
  },
];

export default function DemosIndexPage() {
  return (
    <article className="mx-auto max-w-5xl space-y-8">
      <header className="space-y-3">
        <h2 className="text-3xl font-semibold tracking-tight text-gray-100">
          Demos
        </h2>
        <p className="max-w-3xl text-slate-400">
          Live demos backed by self-hosted services. Each capability is exposed
          through LupiraWeb&apos;s API; the model behind it is an implementation
          detail.
        </p>
      </header>

      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {demos.map((d) => (
          <Link
            key={d.href}
            href={d.href}
            className="group block no-underline"
          >
            <SectionCard title={d.title}>
              <p className="text-slate-400 group-hover:text-slate-200 transition-colors">
                {d.description}
              </p>
            </SectionCard>
          </Link>
        ))}
      </div>
    </article>
  );
}
