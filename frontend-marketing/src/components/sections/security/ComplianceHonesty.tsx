import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { Check, Clock } from "lucide-react";

const today = [
  "OWASP Top 10 & OWASP ASVS L2 baseline",
  "OWASP API Security Top 10",
  "CIS Benchmarks alignment",
  "GDPR-ready data subject workflows (access, export, erasure)",
  "PCI scope minimized — card data is handled entirely by Stripe",
];

const roadmap = [
  "SOC 2 Type II audit",
  "ISO 27001 certification",
  "Third-party penetration testing",
  "Public bug bounty program",
  "Regional data residency for enterprise",
];

export function ComplianceHonesty() {
  return (
    <Section className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Compliance posture</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          What we align with today, and what&apos;s ahead
        </h2>
        <p className="mt-4 max-w-2xl text-muted">
          We&apos;d rather tell you exactly where we stand than claim a certification we don&apos;t
          have yet. MFA and enterprise SSO are architected in but not all shipped in V1.
        </p>
      </Reveal>

      <div className="mt-14 grid gap-6 lg:grid-cols-2">
        <Reveal>
          <GlassCard className="h-full">
            <h3 className="text-sm font-medium text-accent-cyan">Aligned with today</h3>
            <ul className="mt-4 space-y-3">
              {today.map((item) => (
                <li key={item} className="flex items-start gap-2 text-sm">
                  <Check size={16} className="mt-0.5 shrink-0 text-accent-cyan" />
                  <span className="text-muted">{item}</span>
                </li>
              ))}
            </ul>
          </GlassCard>
        </Reveal>
        <Reveal delay={0.06}>
          <GlassCard className="h-full">
            <h3 className="text-sm font-medium text-accent-violet">On our roadmap</h3>
            <ul className="mt-4 space-y-3">
              {roadmap.map((item) => (
                <li key={item} className="flex items-start gap-2 text-sm">
                  <Clock size={16} className="mt-0.5 shrink-0 text-accent-violet" />
                  <span className="text-muted">{item}</span>
                </li>
              ))}
            </ul>
          </GlassCard>
        </Reveal>
      </div>
    </Section>
  );
}
