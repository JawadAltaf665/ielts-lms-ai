import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { GraduationCap, Building2, Globe2, Target, BrainCircuit } from "lucide-react";

const pillars = [
  {
    icon: Target,
    title: "Exam-realistic",
    description: "A computer-based simulator built to mirror the real IELTS test experience, not a generic quiz app.",
  },
  {
    icon: BrainCircuit,
    title: "AI-native",
    description: "Specialized AI agents evaluate, coach, and plan — not a single bolted-on chatbot.",
  },
  {
    icon: GraduationCap,
    title: "Outcome-driven",
    description: "Every feature exists to move your band score, tracked with analytics and confidence-scored predictions.",
  },
  {
    icon: Building2,
    title: "Multi-tenant by design",
    description: "The same platform an individual uses is what institutes and enterprises run their programs on.",
  },
  {
    icon: Globe2,
    title: "Globally scalable",
    description: "Built for learners preparing for study-abroad, migration, and professional registration worldwide.",
  },
];

export function PlatformPillars() {
  return (
    <Section id="platform" className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Platform</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Five principles behind every decision we make
        </h2>
      </Reveal>

      <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {pillars.map((pillar, i) => (
          <Reveal key={pillar.title} delay={i * 0.06}>
            <GlassCard className="h-full">
              <pillar.icon className="text-accent-cyan" size={22} />
              <h3 className="mt-4 text-lg font-medium">{pillar.title}</h3>
              <p className="mt-2 text-sm text-muted">{pillar.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
