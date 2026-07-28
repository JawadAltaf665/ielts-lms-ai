import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { GradientText } from "@/components/ui/GradientText";
import { Reveal } from "@/components/ui/Reveal";
import { Container } from "@/components/ui/Container";
import { HeroScene } from "@/components/3d/HeroScene";
import { Sparkles } from "lucide-react";

export function Hero() {
  return (
    <section className="relative overflow-hidden pt-40 pb-20 sm:pt-48 sm:pb-28">
      <div className="aurora-bg" aria-hidden />
      <Container className="relative z-10 grid items-center gap-16 lg:grid-cols-2">
        <div>
          <Reveal>
            <Badge>
              <Sparkles size={13} /> AI-Native IELTS Platform
            </Badge>
          </Reveal>

          <Reveal delay={0.08}>
            <h1 className="mt-6 text-4xl font-semibold tracking-tight sm:text-5xl lg:text-6xl">
              Prepare for IELTS with <GradientText as="span">an AI that examines you</GradientText> like the real exam does
            </h1>
          </Reveal>

          <Reveal delay={0.16}>
            <p className="mt-6 max-w-xl text-lg text-muted">
              A full computer-based IELTS simulator, AI-evaluated Writing and Speaking, band
              prediction, and a daily study plan that adapts to you — built for individual
              learners and, on the same platform, entire institutions.
            </p>
          </Reveal>

          <Reveal delay={0.24}>
            <div className="mt-10 flex flex-wrap items-center gap-4">
              <Button href="/book-demo" size="lg">
                Book a demo
              </Button>
              <Button href="/pricing" variant="secondary" size="lg">
                Start free
              </Button>
            </div>
          </Reveal>

          <Reveal delay={0.32}>
            <p className="mt-8 text-xs text-muted">
              IELTS is a trademark of its respective owners. IELTS LMS AI is an independent
              preparation platform and is not affiliated with or endorsed by the official test
              owners.
            </p>
          </Reveal>
        </div>

        <div className="relative h-[380px] sm:h-[460px] lg:h-[520px]">
          <HeroScene />
        </div>
      </Container>
    </section>
  );
}
