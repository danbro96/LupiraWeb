import type { NextConfig } from "next";

const config: NextConfig = {
  output: "standalone",
  async rewrites() {
    const api = process.env.API_BASE_URL ?? "http://localhost:5188";
    return [
      {
        source: "/api/:path*",
        destination: `${api}/api/:path*`,
      },
    ];
  },
};

export default config;
