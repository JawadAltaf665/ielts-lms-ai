import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Check } from "lucide-react";

const tiers = [
  {
    name: "Free",
    description: "Try the platform with limited practice and a trial AI writing evaluation.",
    features: ["Grammar & vocabulary practice", "Limited reading & listening", "Trial AI writing evaluation"],
    cta: "Start free",
    href: "/pricing",
    featured: false,
  },
  {
    name: "Premium",
    description: "Full AI evaluation, unlimited mock exams, and band prediction for serious learners.",
    features: ["Full AI writing & speaking evaluation", "Unlimited mock exams", "AI band prediction & study planner"],
    cta: "View plans",
    href: "/pricing",
    featured: true,
  },
  {
    name: "Institute / Enterprise",
    description: "Seat-based billing for institutes, custom contracts and dedicated isolation for enterprise.",
    features: ["Teacher & cohort tools", "Institute-wide analytics", "Enterprise SSO on request"],
    cta: "Contact sales",
    href: "/contact",
    featured: false,
  },
];

export function PricingPreview() {
  return (
    <Section id="pricing" className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Pricing</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Free to start. Built to scale to an institution.
        </h2>
      </Reveal>

      <div className="mt-14 grid gap-6 lg:grid-cols-3">
        {tiers.map((tier, i) => (
          <Reveal key={tier.name} delay={i * 0.08}>
            <GlassCard glow={tier.featured} className="flex h-full flex-col">
              <h3 className="text-lg font-medium">{tier.name}</h3>
              <p className="mt-2 text-sm text-muted">{tier.description}</p>
              <ul className="mt-6 flex-1 space-y-3">
                {tier.features.map((feature) => (
                  <li key={feature} className="flex items-start gap-2 text-sm">
                    <Check size={16} className="mt-0.5 shrink-0 text-accent-cyan" />
                    <span className="text-muted">{feature}</span>
                  </li>
                ))}
              </ul>
              <Button
                href={tier.href}
                variant={tier.featured ? "primary" : "secondary"}
                className="mt-8 w-full"
              >
                {tier.cta}
              </Button>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
