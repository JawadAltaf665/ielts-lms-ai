import { Hero } from "@/components/sections/Hero";
import { Stats } from "@/components/sections/Stats";
import { PlatformPillars } from "@/components/sections/PlatformPillars";
import { FeatureShowcase } from "@/components/sections/FeatureShowcase";
import { AIFeatures } from "@/components/sections/AIFeatures";
import { HowItWorks } from "@/components/sections/HowItWorks";
import { Enterprise } from "@/components/sections/Enterprise";
import { PricingPreview } from "@/components/sections/PricingPreview";
import { WhyChooseUs } from "@/components/sections/WhyChooseUs";
import { FAQ } from "@/components/sections/FAQ";
import { CTA } from "@/components/sections/CTA";

export default function Home() {
  return (
    <>
      <Hero />
      <Stats />
      <PlatformPillars />
      <FeatureShowcase />
      <AIFeatures />
      <HowItWorks />
      <Enterprise />
      <PricingPreview />
      <WhyChooseUs />
      <FAQ />
      <CTA />
    </>
  );
}
