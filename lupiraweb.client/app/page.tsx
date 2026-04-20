import ContactCard from "@/src/components/ContactCard";
import { getMyInfo } from "@/src/api/lupiraWebServerApi";
import type { MyInfo } from "@/src/api/models";

export default async function LandingPage() {
  let me: MyInfo | null = null;
  let error: string | null = null;

  try {
    const res = await getMyInfo();
    if (res.status === 200) me = res.data;
  } catch (err) {
    error = err instanceof Error ? err.message : String(err);
  }

  return (
    <section className="space-y-6">
      <h1 className="text-3xl font-semibold">
        {me?.fullName ? `Hi, I'm ${me.fullName}` : "Welcome"}
      </h1>

      {me?.tagline && (
        <p className="text-slate-300 text-lg">{me.tagline}</p>
      )}

      <p className="text-slate-400 max-w-2xl">
        {me?.bio ??
          "Welcome to my corner! I'm Daniel, and I love tinkering with code, building fun projects, and sharing photos of my cat. I host this site with the hope of sharing things I come across. Feel free to explore and get in touch!"}
      </p>

      {error && <p className="text-sm text-red-500">Error: {error}</p>}

      <div className="space-y-2">
        <h3 className="text-lg font-medium">Contact me</h3>
        <ContactCard />
      </div>
    </section>
  );
}
