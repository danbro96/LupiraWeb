"use client";

import { useEffect, useState } from "react";
import SectionCard from "@/src/components/SectionCard";
import { demoTextToSpeechGetVoices } from "@/src/api/lupiraWebServerApi";
import type { Voice } from "@/src/api/models";
import { synthesizeSpeech } from "@/src/api/demos";

export default function TextToSpeechDemoPage() {
  const [voices, setVoices] = useState<Voice[]>([]);
  const [voiceId, setVoiceId] = useState<string>("");
  const [text, setText] = useState("Hello from LupiraWeb.");
  const [speed, setSpeed] = useState(1.0);
  const [audioUrl, setAudioUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    demoTextToSpeechGetVoices()
      .then((res) => {
        if (cancelled) return;
        setVoices(res.data);
        if (res.data.length > 0) setVoiceId(res.data[0].id);
      })
      .catch((err) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : String(err));
      });
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    return () => {
      if (audioUrl) URL.revokeObjectURL(audioUrl);
    };
  }, [audioUrl]);

  async function handleSynthesize() {
    if (!text.trim() || loading) return;
    setLoading(true);
    setError(null);
    if (audioUrl) {
      URL.revokeObjectURL(audioUrl);
      setAudioUrl(null);
    }
    try {
      const blob = await synthesizeSpeech({
        text,
        voice: voiceId || undefined,
        speed,
      });
      setAudioUrl(URL.createObjectURL(blob));
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setLoading(false);
    }
  }

  return (
    <article className="mx-auto max-w-3xl space-y-8">
      <header className="space-y-2">
        <h2 className="text-3xl font-semibold tracking-tight text-gray-100">
          Text-to-Speech
        </h2>
        <p className="text-slate-400">
          Pick a voice, type some text, and play the synthesized audio.
        </p>
      </header>

      <SectionCard title="Synthesize">
        <div className="space-y-4">
          <textarea
            value={text}
            onChange={(e) => setText(e.target.value)}
            rows={4}
            maxLength={4000}
            className="w-full rounded-md border border-slate-700 bg-slate-950/40 p-3 text-gray-200 focus:border-teal-500 focus:outline-none"
          />

          <div className="grid gap-4 sm:grid-cols-2">
            <label className="block">
              <span className="mb-1 block text-sm text-slate-400">Voice</span>
              <select
                value={voiceId}
                onChange={(e) => setVoiceId(e.target.value)}
                className="w-full rounded-md border border-slate-700 bg-slate-950/40 p-2 text-gray-200 focus:border-teal-500 focus:outline-none"
              >
                {voices.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.name} — {v.language} ({v.gender})
                  </option>
                ))}
              </select>
            </label>

            <label className="block">
              <span className="mb-1 block text-sm text-slate-400">
                Speed: {speed.toFixed(2)}×
              </span>
              <input
                type="range"
                min={0.5}
                max={2.0}
                step={0.05}
                value={speed}
                onChange={(e) => setSpeed(Number(e.target.value))}
                className="w-full"
              />
            </label>
          </div>

          <button
            type="button"
            onClick={handleSynthesize}
            disabled={loading || !text.trim()}
            className="rounded bg-teal-600 px-4 py-2 text-white hover:bg-teal-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {loading ? "Synthesizing…" : "Synthesize"}
          </button>
        </div>
      </SectionCard>

      {error && (
        <SectionCard title="Error">
          <p className="text-red-400">{error}</p>
        </SectionCard>
      )}

      {audioUrl && (
        <SectionCard title="Output">
          <audio src={audioUrl} controls className="w-full" />
        </SectionCard>
      )}
    </article>
  );
}
