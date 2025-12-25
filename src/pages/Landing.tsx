import ContactCard from "../components/ContactCard.tsx";

export default function Landing() {
  return (
    <section className="space-y-6">
      <h1 className="text-3xl font-semibold">Daniel Broström</h1>
      <p className="text-slate-400">
        Full-stack developer, DevOps-oriented, mechanical engineering
        background.
      </p>

    <div className="space-y-2">
        <h3 className="text-lg font-medium">Contact</h3>
        <ContactCard />
      </div>
    </section>
  );
}
