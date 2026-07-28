import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { BrainCircuit, Monitor, ShieldCheck, Accessibility, Cloud, TrendingUp } from "lucide-react";

const principles = [
  { icon: BrainCircuit, title: "AI-first", description: "AI is core to the product, not a bolted-on feature." },
  { icon: Monitor, title: "Computer-based IELTS experience", description: "Built to mirror the real exam interface and timing." },
  { icon: ShieldCheck, title: "Security by default", description: "Tenant isolation and encryption designed in from day one." },
  { icon: Accessibility, title: "Accessible", description: "Built toward WCAG 2.1 AA for every learner-facing screen." },
  { icon: Cloud, title: "Cloud-native", description: "Horizontally scalable, containerized, cloud-agnostic infrastructure." },
  { icon: TrendingUp, title: "Outcome-driven", description: "Every feature is justified by its effect on your estimated band." },
];

export function PrinciplesGrid() {
  return (
    <Section className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Principles</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          What we optimize for
        </h2>
      </Reveal>

      <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {principles.map((principle, i) => (
          <Reveal key={principle.title} delay={(i % 3) * 0.06}>
            <GlassCard className="h-full">
              <principle.icon className="text-accent-cyan" size={22} />
              <h3 className="mt-4 text-base font-medium">{principle.title}</h3>
              <p className="mt-2 text-sm text-muted">{principle.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
