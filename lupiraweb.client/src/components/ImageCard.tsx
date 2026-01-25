import { useState } from "react";

interface ImageCardProps {
  src: string;
  alt: string;
}

export default function ImageCard({ src, alt }: ImageCardProps) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <>
      <img
        src={src}
        alt={alt}
        onClick={() => setIsOpen(true)}
        className="w-full h-40 object-cover rounded cursor-pointer hover:opacity-80 transition-opacity"
      />
      {isOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 p-4"
          onClick={() => setIsOpen(false)}
        >
          <img
            src={src}
            alt={alt}
            className="max-w-full max-h-full object-contain"
            onClick={(e) => e.stopPropagation()} // Prevent closing when clicking the image
          />
          <button
            onClick={() => setIsOpen(false)}
            className="absolute top-4 right-4 text-white text-2xl hover:text-gray-300"
          >
            ×
          </button>
        </div>
      )}
    </>
  );
}
