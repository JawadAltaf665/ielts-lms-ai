import { Badge } from "@/components/ui/Badge";
import { Reveal } from "@/components/ui/Reveal";
import { Container } from "@/components/ui/Container";
import { GradientText } from "@/components/ui/GradientText";
import { Info } from "lucide-react";

export function AboutHero() {
  return (
    <section className="relative overflow-hidden pt-40 pb-16 sm:pt-48">
      <div className="aurora-bg" aria-hidden />
      <Container className="relative z-10 max-w-3xl">
        <Reveal>
          <Badge>
            <Info size={13} /> About
          </Badge>
        </Reveal>
        <Reveal delay={0.08}>
          <h1 className="mt-6 text-4xl font-semibold tracking-tight sm:text-5xl">
            One of the world&apos;s most comprehensive <GradientText>AI-powered IELTS</GradientText> preparation platforms
          </h1>
        </Reveal>
        <Reveal delay={0.16}>
          <p className="mt-6 text-lg text-muted">
            Enabling any learner to prepare for the official computer-based IELTS exam entirely
            within one platform — and any institution to deliver world-class IELTS training
            without building the underlying technology themselves.
          </p>
        </Reveal>
      </Container>
    </section>
  );
}
