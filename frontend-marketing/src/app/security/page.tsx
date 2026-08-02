import type { Metadata } from "next";
import { SecurityHero } from "@/components/sections/security/SecurityHero";
import { IsolationLayers } from "@/components/sections/security/IsolationLayers";
import { AuthHighlights } from "@/components/sections/security/AuthHighlights";
import { TrustStats } from "@/components/sections/security/TrustStats";
import { ComplianceHonesty } from "@/components/sections/security/ComplianceHonesty";
import { CTA } from "@/components/sections/CTA";

export const metadata: Metadata = {
  title: "Security",
  description:
    "Multi-tenant data isolation at every layer, enterprise-grade IAM and encryption, and an honest compliance posture — what's live today and what's on our roadmap.",
};

export default function SecurityPage() {
  return (
    <>
      <SecurityHero />
      <IsolationLayers />
      <AuthHighlights />
      <TrustStats />
      <ComplianceHonesty />
      <CTA />
    </>
  );
}
