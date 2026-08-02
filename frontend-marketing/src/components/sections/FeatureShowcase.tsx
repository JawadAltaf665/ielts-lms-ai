import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import {
  BookOpen,
  Headphones,
  PenLine,
  Mic,
  SpellCheck,
  Layers,
  ClipboardCheck,
  BarChart3,
  Award,
} from "lucide-react";

const features = [
  { icon: BookOpen, title: "Reading", description: "Timed passages with exam-style question types and instant scoring." },
  { icon: Headphones, title: "Listening", description: "Full-length audio tests replicating real IELTS listening sections." },
  { icon: PenLine, title: "Writing", description: "Task 1 & 2 essays scored by AI against IELTS criteria, with feedback." },
  { icon: Mic, title: "Speaking", description: "Recorded responses evaluated for fluency, lexical range, grammar, and pronunciation." },
  { icon: SpellCheck, title: "Grammar & Vocabulary", description: "Targeted drills that adapt to the mistakes you actually make." },
  { icon: Layers, title: "Sentence Builder", description: "Structured practice for constructing accurate, natural sentences." },
  { icon: ClipboardCheck, title: "Mock Exams", description: "Monthly full-length CB-IELTS simulations across all four skills." },
  { icon: BarChart3, title: "Progress Analytics", description: "See exactly which skills are moving your estimated band score." },
  { icon: Award, title: "Certificates", description: "Verifiable completion certificates with unique, checkable IDs." },
];

export function FeatureShowcase() {
  return (
    <Section id="features" className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Everything you need</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          One platform, every IELTS skill
        </h2>
      </Reveal>

      <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {features.map((feature, i) => (
          <Reveal key={feature.title} delay={(i % 3) * 0.06}>
            <GlassCard className="h-full">
              <feature.icon className="text-accent-violet" size={22} />
              <h3 className="mt-4 text-lg font-medium">{feature.title}</h3>
              <p className="mt-2 text-sm text-muted">{feature.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
