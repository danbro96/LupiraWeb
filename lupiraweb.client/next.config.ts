import type { NextConfig } from "next";

const config: NextConfig = {
  output: "standalone",
  reactCompiler: { panicThreshold: "all_errors" },
};

export default config;
