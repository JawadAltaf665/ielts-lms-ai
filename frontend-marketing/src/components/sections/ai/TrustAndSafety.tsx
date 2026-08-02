import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { Gauge, GitBranch, Boxes } from "lucide-react";

const points = [
  {
    icon: Gauge,
    title: "Confidence, not just a number",
    description: "Band Predictor output includes a confidence interval — a wide interval means we recommend more practice instead of a false-precision guess.",
  },
  {
    icon: GitBranch,
    title: "Versioned & regression-tested prompts",
    description: "Prompt changes are versioned and evaluated offline for accuracy and drift before rollout; regressions block release.",
  },
  {
    icon: Boxes,
    title: "Model-agnostic by design",
    description: "Built on Claude and OpenAI today, with room to route to Gemini, Azure OpenAI, or self-hosted models — never locked to one vendor.",
  },
];

export function TrustAndSafety() {
  return (
    <Section aurora className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Trust & safety</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Built to be trusted, not just impressive
        </h2>
      </Reveal>

      <div className="mt-14 grid gap-6 sm:grid-cols-3">
        {points.map((point, i) => (
          <Reveal key={point.title} delay={i * 0.08}>
            <GlassCard className="h-full">
              <point.icon className="text-accent-violet" size={22} />
              <h3 className="mt-4 text-base font-medium">{point.title}</h3>
              <p className="mt-2 text-sm text-muted">{point.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>

      <Reveal delay={0.2}>
        <p className="mt-8 max-w-2xl text-xs text-muted">
          AI-generated band scores and feedback are estimates for practice purposes and are not
          official IELTS results. IELTS is a trademark of its respective owners.
        </p>
      </Reveal>
    </Section>
  );
}
