import { Badge } from "@/components/ui/Badge";
import { Reveal } from "@/components/ui/Reveal";
import { Container } from "@/components/ui/Container";
import { Wallet } from "lucide-react";

export function PricingHero() {
  return (
    <section className="relative overflow-hidden pt-40 pb-16 sm:pt-48">
      <div className="aurora-bg" aria-hidden />
      <Container className="relative z-10 max-w-3xl">
        <Reveal>
          <Badge>
            <Wallet size={13} /> Pricing
          </Badge>
        </Reveal>
        <Reveal delay={0.08}>
          <h1 className="mt-6 text-4xl font-semibold tracking-tight sm:text-5xl">
            Free to start. Built to scale to an institution.
          </h1>
        </Reveal>
        <Reveal delay={0.16}>
          <p className="mt-6 text-lg text-muted">
            Every plan runs on the same platform — from a single learner to a university enrolling
            thousands. Final pricing is confirmed with sales for Institute and Enterprise; talk to
            us for exact figures.
          </p>
        </Reveal>
      </Container>
    </section>
  );
}
