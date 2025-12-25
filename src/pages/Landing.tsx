import ContactCard from "../components/ContactCard.tsx";

export default function Landing() {
  return (
    <section className="space-y-6">
      <h1 className="text-3xl font-semibold">Daniel Broström</h1>
      <p className="text-slate-400">
        Full-stack developer, DevOps-oriented, mechanical engineering
        background.
      </p>

      <ContactCard />
    </section>
  );
}
