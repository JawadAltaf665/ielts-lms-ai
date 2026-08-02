import type { Metadata } from "next";
import { PricingHero } from "@/components/sections/pricing/PricingHero";
import { PricingTiers } from "@/components/sections/pricing/PricingTiers";
import { BillingModel } from "@/components/sections/pricing/BillingModel";
import { PricingFAQ } from "@/components/sections/pricing/PricingFAQ";
import { CTA } from "@/components/sections/CTA";

export const metadata: Metadata = {
  title: "Pricing",
  description:
    "Free, Starter, Premium, Institute, and Enterprise plans on one platform — self-serve billing for individuals, seat-based invoicing for institutes, and custom contracts for enterprise.",
};

export default function PricingPage() {
  return (
    <>
      <PricingHero />
      <PricingTiers />
      <BillingModel />
      <PricingFAQ />
      <CTA />
    </>
  );
}
