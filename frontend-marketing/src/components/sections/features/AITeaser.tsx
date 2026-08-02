import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { FileText, AudioLines, TrendingUp } from "lucide-react";

const items = [
  { icon: FileText, title: "Writing Evaluation", description: "Scored per IELTS criterion, with human review on low-confidence results." },
  { icon: AudioLines, title: "Speaking Evaluation", description: "Fluency, lexical resource, grammar, and pronunciation, scored from your recording." },
  { icon: TrendingUp, title: "Band Prediction", description: "An overall band estimate with a confidence interval, not a bare guess." },
];

export function AITeaser() {
  return (
    <Section aurora className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Powered by 20+ specialized agents</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Every evaluation is backed by dedicated AI examiner agents
        </h2>
      </Reveal>

      <div className="mt-12 grid gap-6 sm:grid-cols-3">
        {items.map((item, i) => (
          <Reveal key={item.title} delay={i * 0.08}>
            <GlassCard className="h-full">
              <item.icon className="text-accent-cyan" size={22} />
              <h3 className="mt-4 text-base font-medium">{item.title}</h3>
              <p className="mt-2 text-sm text-muted">{item.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>

      <Reveal delay={0.24}>
        <div className="mt-10">
          <Button href="/ai" variant="secondary">
            See the full AI architecture
          </Button>
        </div>
      </Reveal>
    </Section>
  );
}
