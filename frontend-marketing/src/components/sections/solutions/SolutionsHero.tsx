import { Badge } from "@/components/ui/Badge";
import { Reveal } from "@/components/ui/Reveal";
import { Container } from "@/components/ui/Container";
import { Users } from "lucide-react";

export function SolutionsHero() {
  return (
    <section className="relative overflow-hidden pt-40 pb-16 sm:pt-48">
      <div className="aurora-bg" aria-hidden />
      <Container className="relative z-10 max-w-3xl">
        <Reveal>
          <Badge>
            <Users size={13} /> Solutions
          </Badge>
        </Reveal>
        <Reveal delay={0.08}>
          <h1 className="mt-6 text-4xl font-semibold tracking-tight sm:text-5xl">
            One platform, built for every seat at the table
          </h1>
        </Reveal>
        <Reveal delay={0.16}>
          <p className="mt-6 text-lg text-muted">
            Students, teachers, institute admins, and enterprise owners all work in the same
            system — no separate tools to reconcile.
          </p>
        </Reveal>
      </Container>
    </section>
  );
}
