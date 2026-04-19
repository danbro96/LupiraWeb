import { test, expect } from "@playwright/test";

const routes: { path: string; title: RegExp; status: number }[] = [
  { path: "/", title: /Lupira$/, status: 200 },
  { path: "/about", title: /^About - Lupira$/, status: 200 },
  { path: "/projects", title: /^Projects - Lupira$/, status: 200 },
  { path: "/projects/example", title: /^Project Example - Lupira$/, status: 200 },
  { path: "/kokos", title: /^Kokos - Lupira$/, status: 200 },
  { path: "/cookies", title: /^Cookies - Lupira$/, status: 200 },
];

for (const { path, title, status } of routes) {
  test(`${path} renders with correct title`, async ({ page }) => {
    const response = await page.goto(path);
    expect(response?.status()).toBe(status);
    await expect(page).toHaveTitle(title);
  });
}

test("unknown path serves the not-found page", async ({ page }) => {
  const response = await page.goto("/does-not-exist");
  expect(response?.status()).toBe(404);
  await expect(page.getByRole("heading", { name: "404" })).toBeVisible();
});

test("landing page renders a weather forecast from the backend", async ({
  page,
}) => {
  await page.goto("/");
  await expect(page.getByRole("heading", { name: "Weather forecast" })).toBeVisible();
  // The minimal API always returns 5 items.
  const items = page.locator("main ul > li");
  await expect(items).toHaveCount(5);
});
