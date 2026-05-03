"use client";

import { useState } from "react";
import SectionCard from "@/src/components/SectionCard";
import { demoChatSendMessage } from "@/src/api/lupiraWebServerApi";

export default function ChatDemoPage() {
  const [prompt, setPrompt] = useState("");
  const [reply, setReply] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSend() {
    if (!prompt.trim() || loading) return;
    setLoading(true);
    setError(null);
    setReply(null);
    try {
      const res = await demoChatSendMessage({ prompt });
      setReply(res.data.reply);
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
          Chat
        </h2>
        <p className="text-slate-400">
          Send a single prompt and read the reply. No conversation history.
        </p>
      </header>

      <SectionCard title="Prompt">
        <div className="space-y-4">
          <textarea
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
            placeholder="Ask something..."
            rows={5}
            className="w-full rounded-md border border-slate-700 bg-slate-950/40 p-3 text-gray-200 focus:border-teal-500 focus:outline-none"
          />
          <button
            type="button"
            onClick={handleSend}
            disabled={loading || !prompt.trim()}
            className="rounded bg-teal-600 px-4 py-2 text-white hover:bg-teal-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {loading ? "Sending…" : "Send"}
          </button>
        </div>
      </SectionCard>

      {error && (
        <SectionCard title="Error">
          <p className="text-red-400">{error}</p>
        </SectionCard>
      )}

      {reply && (
        <SectionCard title="Reply">
          <pre className="whitespace-pre-wrap font-sans text-slate-200">
            {reply}
          </pre>
        </SectionCard>
      )}
    </article>
  );
}
