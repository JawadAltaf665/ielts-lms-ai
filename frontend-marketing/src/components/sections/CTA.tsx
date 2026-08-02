import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Button } from "@/components/ui/Button";
import { GradientText } from "@/components/ui/GradientText";

export function CTA() {
  return (
    <Section className="border-t border-white/[0.06]">
      <Reveal>
        <GlassCard glow className="flex flex-col items-center gap-6 py-16 text-center">
          <h2 className="max-w-xl text-3xl font-semibold tracking-tight sm:text-4xl">
            Ready to move your <GradientText>band score</GradientText>?
          </h2>
          <p className="max-w-md text-muted">
            Start free as a learner, or book a demo to bring your institute or enterprise program onto the platform.
          </p>
          <div className="flex flex-wrap items-center justify-center gap-4">
            <Button href="/pricing" size="lg">
              Start free
            </Button>
            <Button href="/book-demo" variant="secondary" size="lg">
              Book a demo
            </Button>
          </div>
        </GlassCard>
      </Reveal>
    </Section>
  );
}
