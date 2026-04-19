import type { Metadata } from "next";
import ImageCard from "@/src/components/ImageCard";

export const metadata: Metadata = {
  title: "Kokos",
};

export default function KokosPage() {
  const images = Array.from({ length: 8 }, (_, i) => `kokos_${i + 1}.jpg`);

  return (
    <section className="space-y-4">
      <h2 className="text-2xl font-semibold">Kokos</h2>
      <p className="text-slate-400">The most important part.</p>

      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        {images.map((image, index) => (
          <ImageCard
            key={index}
            src={`/kokos/${image}`}
            alt={`Kokos ${index + 1}`}
          />
        ))}
      </div>
    </section>
  );
}
