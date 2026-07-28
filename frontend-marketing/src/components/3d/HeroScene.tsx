"use client";

import dynamic from "next/dynamic";
import { useMediaQuery } from "@/lib/useMediaQuery";

const NeuralOrbCanvas = dynamic(() => import("./NeuralOrbCanvas"), {
  ssr: false,
  loading: () => <StaticOrbFallback />,
});

function StaticOrbFallback() {
  return (
    <div className="relative h-full w-full">
      <div className="absolute left-1/2 top-1/2 h-64 w-64 -translate-x-1/2 -translate-y-1/2 rounded-full bg-gradient-to-br from-accent-indigo via-accent-violet to-accent-cyan opacity-40 blur-3xl" />
    </div>
  );
}

export function HeroScene() {
  const enable3d = useMediaQuery("(min-width: 640px) and (prefers-reduced-motion: no-preference)");

  if (!enable3d) return <StaticOrbFallback />;
  return <NeuralOrbCanvas />;
}
