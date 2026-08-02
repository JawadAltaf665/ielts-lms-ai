import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Button } from "@/components/ui/Button";
import { Check } from "lucide-react";

const tiers = [
  {
    name: "Free",
    tagline: "Try the platform",
    features: ["Grammar & vocabulary practice", "Limited reading & listening", "Trial AI writing evaluation"],
    cta: "Start free",
    href: "/book-demo",
    featured: false,
  },
  {
    name: "Starter",
    tagline: "Serious individual learners",
    features: ["Full AI writing & speaking evaluation", "Limited monthly mock exams", "AI band prediction included"],
    cta: "Choose Starter",
    href: "/book-demo",
    featured: false,
  },
  {
    name: "Premium",
    tagline: "Power individual learners",
    features: ["Unlimited AI writing & speaking evaluation", "Unlimited mock exams", "AI daily study planner"],
    cta: "Choose Premium",
    href: "/book-demo",
    featured: true,
  },
  {
    name: "Institute",
    tagline: "Schools & coaching centers",
    features: ["Everything in Premium, per seat", "Teacher & cohort tools", "Institute-wide analytics, optional SSO"],
    cta: "Talk to sales",
    href: "/contact",
    featured: false,
  },
  {
    name: "Enterprise",
    tagline: "Universities & large organizations",
    features: ["Custom contract & SLA terms", "Enterprise SSO (SAML/OIDC)", "Dedicated database isolation on request"],
    cta: "Contact sales",
    href: "/contact",
    featured: false,
  },
];

export function PricingTiers() {
  return (
    <Section className="border-t border-white/[0.06]">
      <div className="grid gap-6 lg:grid-cols-3 xl:grid-cols-5">
        {tiers.map((tier, i) => (
          <Reveal key={tier.name} delay={(i % 3) * 0.06}>
            <GlassCard glow={tier.featured} className="flex h-full flex-col">
              <h3 className="text-lg font-medium">{tier.name}</h3>
              <p className="mt-1 text-sm text-muted">{tier.tagline}</p>
              <ul className="mt-6 flex-1 space-y-3">
                {tier.features.map((feature) => (
                  <li key={feature} className="flex items-start gap-2 text-sm">
                    <Check size={16} className="mt-0.5 shrink-0 text-accent-cyan" />
                    <span className="text-muted">{feature}</span>
                  </li>
                ))}
              </ul>
              <Button href={tier.href} variant={tier.featured ? "primary" : "secondary"} className="mt-8 w-full">
                {tier.cta}
              </Button>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
