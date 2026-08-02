import { Badge } from "@/components/ui/Badge";
import { Reveal } from "@/components/ui/Reveal";
import { Container } from "@/components/ui/Container";
import { Layers } from "lucide-react";

export function FeaturesHero() {
  return (
    <section className="relative overflow-hidden pt-40 pb-16 sm:pt-48">
      <div className="aurora-bg" aria-hidden />
      <Container className="relative z-10 max-w-3xl">
        <Reveal>
          <Badge>
            <Layers size={13} /> Platform features
          </Badge>
        </Reveal>
        <Reveal delay={0.08}>
          <h1 className="mt-6 text-4xl font-semibold tracking-tight sm:text-5xl">
            Every feature your prep needs, on one platform
          </h1>
        </Reveal>
        <Reveal delay={0.16}>
          <p className="mt-6 text-lg text-muted">
            From daily grammar drills to a full computer-based mock exam, every module feeds the
            same progress model — so nothing you practice is disconnected from your estimated band.
          </p>
        </Reveal>
      </Container>
    </section>
  );
}
