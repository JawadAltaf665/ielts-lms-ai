import type { Metadata } from "next";
import { FeaturesHero } from "@/components/sections/features/FeaturesHero";
import { MockExamSpotlight } from "@/components/sections/features/MockExamSpotlight";
import { LearningPractice } from "@/components/sections/features/LearningPractice";
import { AITeaser } from "@/components/sections/features/AITeaser";
import { PlanningProgress } from "@/components/sections/features/PlanningProgress";
import { CollaborationAdmin } from "@/components/sections/features/CollaborationAdmin";
import { CTA } from "@/components/sections/CTA";

export const metadata: Metadata = {
  title: "Features",
  description:
    "Grammar, vocabulary, reading, listening, AI-evaluated writing and speaking, a full computer-based mock exam, and the planning tools that tie them together.",
};

export default function FeaturesPage() {
  return (
    <>
      <FeaturesHero />
      <MockExamSpotlight />
      <LearningPractice />
      <AITeaser />
      <PlanningProgress />
      <CollaborationAdmin />
      <CTA />
    </>
  );
}
