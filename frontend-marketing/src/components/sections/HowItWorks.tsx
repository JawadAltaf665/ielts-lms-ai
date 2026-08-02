import { Section } from "@/components/ui/Section";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { cn } from "@/lib/utils";

const steps = [
  {
    step: "01",
    title: "Assess your starting point",
    description: "A diagnostic mock exam establishes your current estimated band across all four skills.",
  },
  {
    step: "02",
    title: "Get a plan built for you",
    description: "The AI Study Planner turns your target band and deadline into a day-by-day plan.",
  },
  {
    step: "03",
    title: "Practice with real feedback",
    description: "Every essay and recorded response is scored against IELTS criteria, with specific corrections.",
  },
  {
    step: "04",
    title: "Track toward your target band",
    description: "Monthly mock exams and a confidence-scored band prediction show whether you're on track.",
  },
];

export function HowItWorks() {
  return (
    <Section id="how-it-works" className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>How it works</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          From diagnostic to exam day
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
