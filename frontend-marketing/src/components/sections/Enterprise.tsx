import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { ShieldCheck, Users, Building, KeyRound } from "lucide-react";

const points = [
  {
    icon: ShieldCheck,
    title: "Isolation at every layer",
    description: "Every query, cache entry, file, and background job is scoped to your tenant. Cross-tenant access is architecturally blocked, not just permission-checked.",
  },
  {
    icon: Users,
    title: "Teacher & cohort tools",
    description: "Manage classes, assignments, and institute-wide analytics from one dashboard.",
  },
  {
    icon: Building,
    title: "Dedicated isolation, on request",
    description: "Enterprise tenants can move from shared to dedicated database isolation as compliance needs grow.",
  },
  {
    icon: KeyRound,
    title: "Enterprise SSO, on our roadmap",
    description: "SAML 2.0 / OIDC single sign-on (Azure AD, Okta, Google) is architected in and planned for enterprise rollout.",
  },
];

export function Enterprise() {
  return (
    <Section id="enterprise" className="border-t border-white/[0.06]">
      <div className="grid gap-12 lg:grid-cols-2 lg:gap-16">
        <Reveal>
          <Badge>For institutes & enterprise</Badge>
          <h2 className="mt-6 text-3xl font-semibold tracking-tight sm:text-4xl">
            Run your IELTS program on infrastructure you didn&apos;t have to build
          </h2>
          <p className="mt-4 text-muted">
            One consolidated invoice, one admin dashboard, and a data isolation model designed for
            institutions and universities from day one — not bolted on later.
          </p>
          <div className="mt-8">
            <Button href="/solutions#enterprise" variant="secondary">
              See enterprise capabilities
            </Button>
          </div>
        </Reveal>

        <div className="grid gap-6 sm:grid-cols-2">
          {points.map((point, i) => (
            <Reveal key={point.title} delay={i * 0.06}>
              <GlassCard className="h-full">
                <point.icon className="text-accent-indigo" size={20} />
                <h3 className="mt-3 text-base font-medium">{point.title}</h3>
                <p className="mt-2 text-sm text-muted">{point.description}</p>
              </GlassCard>
            </Reveal>
          ))}
        </div>
      </div>
    </Section>
  );
}
