import type { Metadata } from "next";
import { AboutHero } from "@/components/sections/about/AboutHero";
import { Thesis } from "@/components/sections/about/Thesis";
import { PrinciplesGrid } from "@/components/sections/about/PrinciplesGrid";
import { LegalNotice } from "@/components/sections/about/LegalNotice";
import { CTA } from "@/components/sections/CTA";

export const metadata: Metadata = {
  title: "About",
  description:
    "IELTS LMS AI is an AI-native, multi-tenant IELTS preparation platform built for individual learners and institutions alike.",
};

export default function AboutPage() {
  return (
    <>
      <AboutHero />
      <Thesis />
      <PrinciplesGrid />
      <LegalNotice />
      <CTA />
    </>
  );
}
