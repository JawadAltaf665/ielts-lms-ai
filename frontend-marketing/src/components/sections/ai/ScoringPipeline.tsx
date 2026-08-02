import { Section } from "@/components/ui/Section";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { cn } from "@/lib/utils";

const steps = [
  { step: "01", title: "You submit", description: "A written response or a recorded speaking answer, same as exam conditions." },
  { step: "02", title: "The examiner agent scores it", description: "Per-criterion scoring at low temperature, anchored to the IELTS rubric, not freeform." },
  { step: "03", title: "Confidence is checked", description: "Every score carries a confidence signal — it isn't treated as final by default." },
  { step: "04", title: "Low confidence escalates to a human", description: "Disputed or uncertain scores are reviewed by a person before they're marked final." },
];

export function ScoringPipeline() {
  return (
    <Section id="scoring" className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>How scoring works</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          AI first, human oversight where it matters
        </h2>
      </Reveal>

      <div className="mt-16 grid gap-10 lg:grid-cols-4 lg:gap-6">
        {steps.map((step, i) => (
          <Reveal key={step.step} delay={i * 0.08}>
            <div className={cn("relative pl-6 lg:pl-0", i < steps.length - 1 && "lg:border-r lg:border-white/[0.08] lg:pr-6")}>
              <div className="font-mono text-sm text-accent-cyan">{step.step}</div>
              <h3 className="mt-3 text-lg font-medium">{step.title}</h3>
              <p className="mt-2 text-sm text-muted">{step.description}</p>
            </div>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
