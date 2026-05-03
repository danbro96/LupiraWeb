"use client";

import { useEffect, useRef, useState } from "react";
import SectionCard from "@/src/components/SectionCard";
import {
  demoVisionCaption,
  demoVisionDetect,
  demoVisionOcr,
} from "@/src/api/lupiraWebServerApi";
import type { Detection } from "@/src/api/models";

type Task = "caption" | "ocr" | "detect";

interface CaptionResult { kind: "caption"; caption: string }
interface OcrResult { kind: "ocr"; text: string }
interface DetectResult { kind: "detect"; items: Detection[] }
type Result = CaptionResult | OcrResult | DetectResult;

const num = (v: number | string): number =>
  typeof v === "number" ? v : parseFloat(v);

export default function VisionDemoPage() {
  const [file, setFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [task, setTask] = useState<Task>("caption");
  const [result, setResult] = useState<Result | null>(null);
  const [imgDims, setImgDims] = useState<{ w: number; h: number } | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const imgRef = useRef<HTMLImageElement>(null);

  useEffect(() => {
    return () => {
      if (previewUrl) URL.revokeObjectURL(previewUrl);
    };
  }, [previewUrl]);

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const f = e.target.files?.[0] ?? null;
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    setFile(f);
    setPreviewUrl(f ? URL.createObjectURL(f) : null);
    setResult(null);
    setImgDims(null);
    setError(null);
  }

  async function handleSubmit() {
    if (!file || loading) return;
    setLoading(true);
    setError(null);
    setResult(null);
    try {
      if (task === "caption") {
        const res = await demoVisionCaption({ image: file });
        setResult({ kind: "caption", caption: res.data.caption });
      } else if (task === "ocr") {
        const res = await demoVisionOcr({ image: file });
        setResult({ kind: "ocr", text: res.data.text });
      } else {
        const res = await demoVisionDetect({ image: file });
        setResult({ kind: "detect", items: res.data.items });
      }
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
          Vision
        </h2>
        <p className="text-slate-400">
          Upload an image and run a caption, OCR, or object-detection task.
        </p>
      </header>

      <SectionCard title="Image">
        <div className="space-y-4">
          <input
            type="file"
            accept="image/*"
            onChange={handleFileChange}
            className="block w-full text-sm text-slate-400 file:mr-4 file:rounded file:border-0 file:bg-teal-600 file:px-4 file:py-2 file:text-white file:hover:bg-teal-700"
          />

          {previewUrl && (
            <div className="relative inline-block">
              <img
                ref={imgRef}
                src={previewUrl}
                alt="Preview"
                onLoad={(e) =>
                  setImgDims({
                    w: e.currentTarget.naturalWidth,
                    h: e.currentTarget.naturalHeight,
                  })
                }
                className="max-h-96 rounded border border-slate-700"
              />
              {result?.kind === "detect" && imgDims && (
                <svg
                  className="pointer-events-none absolute inset-0 h-full w-full"
                  viewBox={`0 0 ${imgDims.w} ${imgDims.h}`}
                  preserveAspectRatio="none"
                >
                  {result.items.map((d, i) => {
                    const x1 = num(d.x1), y1 = num(d.y1);
                    const x2 = num(d.x2), y2 = num(d.y2);
                    return (
                      <g key={i}>
                        <rect
                          x={x1}
                          y={y1}
                          width={x2 - x1}
                          height={y2 - y1}
                          fill="none"
                          stroke="#5eead4"
                          strokeWidth={Math.max(2, imgDims.w / 300)}
                        />
                        <text
                          x={x1}
                          y={Math.max(y1 - 4, 12)}
                          fill="#5eead4"
                          fontSize={Math.max(12, imgDims.w / 50)}
                        >
                          {d.label}
                        </text>
                      </g>
                    );
                  })}
                </svg>
              )}
            </div>
          )}
        </div>
      </SectionCard>

      <SectionCard title="Task">
        <div className="space-y-4">
          <div className="flex gap-2">
            {(["caption", "ocr", "detect"] as Task[]).map((t) => (
              <button
                key={t}
                type="button"
                onClick={() => setTask(t)}
                className={`rounded px-3 py-1 text-sm ${
                  task === t
                    ? "bg-teal-600 text-white"
                    : "bg-slate-800 text-slate-300 hover:bg-slate-700"
                }`}
              >
                {t === "caption" ? "Caption" : t === "ocr" ? "OCR" : "Detect"}
              </button>
            ))}
          </div>

          <button
            type="button"
            onClick={handleSubmit}
            disabled={!file || loading}
            className="rounded bg-teal-600 px-4 py-2 text-white hover:bg-teal-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {loading ? "Running…" : "Run"}
          </button>
        </div>
      </SectionCard>

      {error && (
        <SectionCard title="Error">
          <p className="text-red-400">{error}</p>
        </SectionCard>
      )}

      {result?.kind === "caption" && (
        <SectionCard title="Caption">
          <p className="text-slate-200">{result.caption}</p>
        </SectionCard>
      )}

      {result?.kind === "ocr" && (
        <SectionCard title="Extracted Text">
          <pre className="whitespace-pre-wrap font-sans text-slate-200">
            {result.text || "(no text found)"}
          </pre>
        </SectionCard>
      )}

      {result?.kind === "detect" && (
        <SectionCard title="Detections">
          {result.items.length === 0 ? (
            <p className="text-slate-400">(none)</p>
          ) : (
            <ul className="list-disc list-inside text-slate-200">
              {result.items.map((d, i) => (
                <li key={i}>
                  {d.label} — ({Math.round(num(d.x1))}, {Math.round(num(d.y1))})
                  → ({Math.round(num(d.x2))}, {Math.round(num(d.y2))})
                </li>
              ))}
            </ul>
          )}
        </SectionCard>
      )}
    </article>
  );
}
