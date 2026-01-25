import ContactCard from "../components/ContactCard";

export default function Landing(): React.ReactElement {
  return (
    <section className="space-y-6">
      <h1 className="text-3xl font-semibold">Welcome</h1>
      <p className="text-slate-400 max-w-2xl">
        Welcome to my corner! I'm Daniel, and I love
        tinkering with code, building fun projects, and sharing photos of my cat.
        I host this site with the hope of sharing things I come across.
        Feel free to explore and get in touch!
      </p>

      <div className="space-y-2">
        <h3 className="text-lg font-medium">Contact me</h3>
        <ContactCard />
      </div>
    </section>
  );
}
