import ContactCard from "@/src/components/ContactCard";
import { getWeatherForecast } from "@/src/api/lupiraWebServerApi";
import type { WeatherForecast } from "@/src/api/models";

export default async function LandingPage() {
  let forecasts: WeatherForecast[] | null = null;
  let error: string | null = null;

  try {
    const res = await getWeatherForecast();
    forecasts = res.data;
  } catch (err) {
    error = err instanceof Error ? err.message : String(err);
  }

  return (
    <section className="space-y-6">
      <h1 className="text-3xl font-semibold">Welcome</h1>
      <p className="text-slate-400 max-w-2xl">
        Welcome to my corner! I&apos;m Daniel, and I love tinkering with code,
        building fun projects, and sharing photos of my cat. I host this site
        with the hope of sharing things I come across. Feel free to explore and
        get in touch!
      </p>

      <div className="space-y-2">
        <h3 className="text-lg font-medium">Contact me</h3>
        <ContactCard />
      </div>

      <div className="pt-4">
        <h3 className="text-lg font-medium">Weather forecast</h3>

        {error && <p className="text-sm text-red-500">Error: {error}</p>}

        {!error &&
          (forecasts && forecasts.length > 0 ? (
            <ul className="mt-2 space-y-2">
              {forecasts.map((f) => (
                <li
                  key={f.date}
                  className="p-3 border rounded-md bg-white/5 flex justify-between"
                >
                  <div>
                    <div className="text-sm font-medium">{f.summary ?? "—"}</div>
                    <div className="text-xs text-slate-400">{f.date}</div>
                  </div>
                  <div className="text-right">
                    <div className="text-sm">{f.temperatureC}°C</div>
                    <div className="text-xs text-slate-400">{f.temperatureF}°F</div>
                  </div>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-sm text-slate-500 mt-2">No forecast available.</p>
          ))}
      </div>
    </section>
  );
}
