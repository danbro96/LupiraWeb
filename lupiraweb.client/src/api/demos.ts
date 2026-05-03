import { getDemoTextToSpeechSynthesizeUrl } from "./lupiraWebServerApi";
import type { SynthesizeRequest } from "./models";

export async function synthesizeSpeech(req: SynthesizeRequest): Promise<Blob> {
  const res = await fetch(getDemoTextToSpeechSynthesizeUrl(), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(req),
  });
  if (!res.ok) {
    throw new Error(`Synthesize failed: ${res.status} ${res.statusText}`);
  }
  return await res.blob();
}
