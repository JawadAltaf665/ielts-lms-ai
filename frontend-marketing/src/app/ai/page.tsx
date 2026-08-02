import type { Metadata } from "next";
import { AIHero } from "@/components/sections/ai/AIHero";
import { AgentCatalog } from "@/components/sections/ai/AgentCatalog";
import { ScoringPipeline } from "@/components/sections/ai/ScoringPipeline";
import { TrustAndSafety } from "@/components/sections/ai/TrustAndSafety";
import { CTA } from "@/components/sections/CTA";

export const metadata: Metadata = {
  title: "AI Architecture",
  description:
    "A coordinator agent delegates to 20+ specialized examiner, coach, and planning agents — rubric-anchored scoring, confidence-scored band prediction, and human review where it matters.",
};

export default function AIPage() {
  return (
    <>
      <AIHero />
      <AgentCatalog />
      <ScoringPipeline />
      <TrustAndSafety />
      <CTA />
    </>
  );
}
