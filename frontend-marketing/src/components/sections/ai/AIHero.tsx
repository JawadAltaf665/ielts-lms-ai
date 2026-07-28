import { Badge } from "@/components/ui/Badge";
import { Reveal } from "@/components/ui/Reveal";
import { Container } from "@/components/ui/Container";
import { BrainCircuit } from "lucide-react";

export function AIHero() {
  return (
    <section className="relative overflow-hidden pt-40 pb-16 sm:pt-48">
      <div className="aurora-bg" aria-hidden />
      <Container className="relative z-10 max-w-3xl">
        <Reveal>
          <Badge>
            <BrainCircuit size={13} /> Agentic AI architecture
          </Badge>
        </Reveal>
        <Reveal delay={0.08}>
          <h1 className="mt-6 text-4xl font-semibold tracking-tight sm:text-5xl">
            One coordinator. Twenty-plus specialized agents.
          </h1>
        </Reveal>
        <Reveal delay={0.16}>
          <p className="mt-6 text-lg text-muted">
            Instead of one general-purpose chatbot guessing at IELTS scores, a coordinator agent
            delegates to dedicated examiner, coach, and planning agents — each scoped to one job,
            with rubric-anchored, low-variance output.
          </p>
        </Reveal>
      </Container>
    </section>
  );
}
