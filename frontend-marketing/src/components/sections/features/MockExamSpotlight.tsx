import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { Check } from "lucide-react";

const bullets = [
  "Full 4-skill simulation: Reading, Listening, Writing, Speaking",
  "Timing and interface conventions that mirror the real computer-based test",
  "Objective sections auto-scored instantly",
  "Writing & Speaking evaluated by dedicated AI examiner agents",
  "Estimated overall band delivered with a confidence interval",
];

export function MockExamSpotlight() {
  return (
    <Section className="border-t border-white/[0.06]">
      <div className="grid items-center gap-12 lg:grid-cols-2 lg:gap-16">
        <Reveal>
          <Badge>Flagship</Badge>
          <h2 className="mt-6 text-3xl font-semibold tracking-tight sm:text-4xl">
            A mock exam that feels like exam day
          </h2>
          <p className="mt-4 text-muted">
            Monthly full-length CB-IELTS simulations run the same way the real computer-based test
            does — so exam day isn&apos;t the first time you&apos;ve seen the interface.
          </p>
          <ul className="mt-6 space-y-3">
            {bullets.map((bullet) => (
              <li key={bullet} className="flex items-start gap-2 text-sm">
                <Check size={16} className="mt-0.5 shrink-0 text-accent-cyan" />
                <span className="text-muted">{bullet}</span>
              </li>
            ))}
          </ul>
        </Reveal>

        <Reveal delay={0.1}>
          <GlassCard glow className="overflow-hidden p-0">
            <div className="flex items-center gap-1.5 border-b border-white/[0.08] px-4 py-3">
              <span className="h-2.5 w-2.5 rounded-full bg-white/20" />
              <span className="h-2.5 w-2.5 rounded-full bg-white/20" />
              <span className="h-2.5 w-2.5 rounded-full bg-white/20" />
              <span className="ml-3 font-mono text-xs text-muted">mock-exam · writing · task 2</span>
            </div>
            <div className="grid gap-8 p-8 sm:grid-cols-[1fr_auto]">
              <div className="space-y-3">
                <div className="h-3 w-full rounded-full bg-white/[0.06]" />
                <div className="h-3 w-11/12 rounded-full bg-white/[0.06]" />
                <div className="h-3 w-full rounded-full bg-white/[0.06]" />
                <div className="h-3 w-4/5 rounded-full bg-white/[0.06]" />
                <div className="h-3 w-10/12 rounded-full bg-white/[0.06]" />
                <div className="mt-6 h-3 w-2/3 rounded-full bg-white/[0.06]" />
                <div className="h-3 w-1/2 rounded-full bg-white/[0.06]" />
              </div>
              <div className="flex flex-col items-center justify-center gap-2">
                <div className="band-ring">7.5</div>
                <span className="text-xs text-muted">Estimated band</span>
              </div>
            </div>
          </GlassCard>
        </Reveal>
      </div>
    </Section>
  );
}
