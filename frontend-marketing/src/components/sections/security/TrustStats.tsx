import { Section } from "@/components/ui/Section";
import { Reveal } from "@/components/ui/Reveal";

const stats = [
  { value: "99.9%", label: "platform uptime target" },
  { value: "99.95%", label: "enterprise SLA target" },
  { value: "≤15 min", label: "recovery point objective (RPO)" },
  { value: "≤4 hrs", label: "recovery time objective (RTO)" },
];

export function TrustStats() {
  return (
    <Section className="border-t border-white/[0.06] py-16 sm:py-20">
      <div className="grid grid-cols-2 gap-8 lg:grid-cols-4">
        {stats.map((stat, i) => (
          <Reveal key={stat.label} delay={i * 0.06}>
            <div className="text-center lg:text-left">
              <div className="font-mono text-3xl font-semibold sm:text-4xl">{stat.value}</div>
              <div className="mt-2 text-sm text-muted">{stat.label}</div>
            </div>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
