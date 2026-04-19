import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import CookiesPage from "./page";

describe("CookiesPage", () => {
  it("renders the no-cookies notice", () => {
    render(<CookiesPage />);
    expect(
      screen.getByRole("heading", { level: 2, name: "Cookies" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/does not use cookies, analytics, trackers/i),
    ).toBeInTheDocument();
  });
});
