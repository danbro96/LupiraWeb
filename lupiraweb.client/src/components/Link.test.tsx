import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import Link from "./Link";

describe("Link", () => {
  it("renders a next/link for internal hrefs", () => {
    render(<Link href="/about">About</Link>);
    const anchor = screen.getByRole("link", { name: "About" });
    expect(anchor).toHaveAttribute("href", "/about");
  });

  it("renders a plain anchor for external hrefs", () => {
    render(<Link href="https://example.com">External</Link>);
    const anchor = screen.getByRole("link", { name: "External" });
    expect(anchor).toHaveAttribute("href", "https://example.com");
  });

  it("applies the shared styling classes", () => {
    render(<Link href="/about">About</Link>);
    const anchor = screen.getByRole("link", { name: "About" });
    expect(anchor).toHaveClass("text-slate-400");
    expect(anchor).toHaveClass("hover:text-teal-300");
  });
});
