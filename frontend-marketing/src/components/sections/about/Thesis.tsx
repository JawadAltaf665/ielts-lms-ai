import { Section } from "@/components/ui/Section";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";

export function Thesis() {
  return (
    <Section className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Why we exist</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          IELTS prep is service-heavy and capacity-constrained. We think it can be software-driven instead.
        </h2>
        <p className="mt-6 max-w-2xl text-muted">
          AI makes high-quality, personalized IELTS preparation scalable and affordable, while
          multi-tenancy makes the platform sellable to institutions as infrastructure — the same
          system a single learner uses is what a university runs its entire program on.
        </p>
        <p className="mt-4 max-w-2xl text-muted">
          We are not a marketplace, not a live-tutoring service, and not a generic study app
          retrofitted for IELTS. Every module exists because it moves a real, estimated band score.
        </p>
      </Reveal>
    </Section>
  );
}
