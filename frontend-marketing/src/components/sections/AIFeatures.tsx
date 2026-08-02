import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Bot, FileText, AudioLines, TrendingUp, CalendarClock } from "lucide-react";

const capabilities = [
  {
    icon: Bot,
    title: "AI Tutor",
    description:
      "Specialized coaching agents for grammar, vocabulary, sentence building, reading, and listening — structured corrections and explanations, not vague chat.",
  },
  {
    icon: FileText,
    title: "AI Writing Evaluation",
    description:
      "Every Task 1 & 2 essay is scored per IELTS criterion with detailed feedback. Low-confidence scores are escalated for human review before they're final.",
  },
  {
    icon: AudioLines,
    title: "AI Speaking Evaluation",
    description:
      "Recorded responses are scored on fluency, lexical resource, grammar, and pronunciation — the same criteria the real exam uses.",
  },
  {
    icon: TrendingUp,
    title: "AI Band Prediction",
    description:
      "A predicted overall band with a confidence interval, not a bare number — a wide interval means we tell you to practice more rather than guess.",
  },
  {
    icon: CalendarClock,
    title: "AI Daily Study Planner",
    description:
      "Your plan adjusts every day based on real performance: progress analysis, recommendations, and today's tasks generated automatically.",
  },
];

export function AIFeatures() {
  return (
    <Section id="ai" aurora className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>AI, done properly</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          20 specialized agents. Not one chatbot.
        </h2>
        <p className="mt-4 max-w-2xl text-muted">
          A coordinator delegates to dedicated examiner, coach, and planning agents — each with its
          own scope, so scoring stays rubric-anchored instead of freeform.
        </p>
      </Reveal>

      <div className="mt-14 grid gap-6 lg:grid-cols-2">
        {capabilities.map((capability, i) => (
          <Reveal key={capability.title} delay={(i % 2) * 0.08}>
            <GlassCard glow className="h-full">
              <capability.icon className="text-accent-cyan" size={24} />
              <h3 className="mt-4 text-lg font-medium">{capability.title}</h3>
              <p className="mt-2 text-sm text-muted">{capability.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>

      <Reveal delay={0.2}>
        <div className="mt-12 flex flex-wrap items-center gap-4">
          <Button href="/ai" variant="secondary">
            Explore the AI architecture
          </Button>
          <p className="text-xs text-muted">
            AI-generated band scores and feedback are estimates for practice, not official IELTS results.
          </p>
        </div>
      </Reveal>
    </Section>
  );
}
