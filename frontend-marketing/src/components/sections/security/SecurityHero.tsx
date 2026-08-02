import { Badge } from "@/components/ui/Badge";
import { Reveal } from "@/components/ui/Reveal";
import { Container } from "@/components/ui/Container";
import { ShieldCheck } from "lucide-react";

export function SecurityHero() {
  return (
    <section className="relative overflow-hidden pt-40 pb-16 sm:pt-48">
      <div className="aurora-bg" aria-hidden />
      <Container className="relative z-10 max-w-3xl">
        <Reveal>
          <Badge>
            <ShieldCheck size={13} /> Security & trust
          </Badge>
        </Reveal>
        <Reveal delay={0.08}>
          <h1 className="mt-6 text-4xl font-semibold tracking-tight sm:text-5xl">
            Multi-tenant isolation, built in from the first line of code
          </h1>
        </Reveal>
        <Reveal delay={0.16}>
          <p className="mt-6 text-lg text-muted">
            Every institute and enterprise tenant is isolated at every layer of the stack —
            not bolted on after the fact. Here&apos;s exactly how, including what&apos;s live today
            and what&apos;s on our roadmap.
          </p>
        </Reveal>
      </Container>
    </section>
  );
}
