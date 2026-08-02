import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { User, Users, Building2 } from "lucide-react";

const models = [
  {
    icon: User,
    title: "Individual",
    description: "Self-serve card billing, monthly or yearly. Cancel or change plans anytime.",
  },
  {
    icon: Users,
    title: "Institution",
    description: "Seat-based pricing with one consolidated invoice — your admin pays, your teachers and students just use it.",
  },
  {
    icon: Building2,
    title: "Enterprise",
    description: "Custom contract terms, annual invoicing, and SLAs — built around procurement, not a credit card form.",
  },
];

export function BillingModel() {
  return (
    <Section className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>How billing works</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Billed the way your organization actually buys software
        </h2>
      </Reveal>

      <div className="mt-14 grid gap-6 sm:grid-cols-3">
        {models.map((model, i) => (
          <Reveal key={model.title} delay={i * 0.08}>
            <GlassCard className="h-full">
              <model.icon className="text-accent-indigo" size={22} />
              <h3 className="mt-4 text-base font-medium">{model.title}</h3>
              <p className="mt-2 text-sm text-muted">{model.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
