import React, { useEffect, useState } from "react";
import ContactCard from "../components/ContactCard";
import { getWeatherForecast } from "../api/lupiraWebServerApi";
import type { WeatherForecast } from "../api/models";

export default function Landing(): React.ReactElement {
    const [forecasts, setForecasts] = useState<WeatherForecast[] | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let mounted = true;
        getWeatherForecast()
            .then((res) => {
                console.log(res.data)
                if (mounted) setForecasts(res.data);
            })
            .catch((err) => {
                if (mounted) setError(err?.message ?? String(err));
            })
            .finally(() => {
                if (mounted) setLoading(false);
            });

        return () => {
            mounted = false;
        };
    }, []);

    return (
        <section className="space-y-6">
            <h1 className="text-3xl font-semibold">Welcome</h1>
            <p className="text-slate-400 max-w-2xl">
                Welcome to my corner! I'm Daniel, and I love tinkering with code,
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

                {loading && <p className="text-sm text-slate-500">Loading weather…</p>}
                {error && <p className="text-sm text-red-500">Error: {error}</p>}

                {!loading && !error && (
                    <>
                        {!!forecasts && forecasts.length > 0 ? (
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
                                            <div className="text-xs text-slate-400">
                                                {f.temperatureF}°F
                                            </div>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        ) : (
                            <p className="text-sm text-slate-500 mt-2">No forecast available.</p>
                        )}
                    </>
                )}
            </div>
        </section>
    );
}