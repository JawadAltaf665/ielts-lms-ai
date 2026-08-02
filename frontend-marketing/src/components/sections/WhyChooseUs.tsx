import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { ScanEye, ShieldCheck, Layers3, Globe } from "lucide-react";

const reasons = [
  {
    icon: ScanEye,
    title: "Rubric-anchored, not generic",
    description: "AI scoring is grounded in IELTS criteria with bounded, low-variance output — not a general-purpose chatbot guessing a number.",
  },
  {
    icon: ShieldCheck,
    title: "Human-in-the-loop where it matters",
    description: "Low-confidence writing and speaking scores are escalated for human review before they're marked final.",
  },
  {
    icon: Layers3,
    title: "One platform, every persona",
    description: "Students, teachers, institute admins, and enterprise owners work in the same system — no separate tools to reconcile.",
  },
  {
    icon: Globe,
    title: "Built for a global exam",
    description: "Designed from the start for learners preparing across regions, not retrofitted from a domestic study app.",
  },
];

export function WhyChooseUs() {
  return (
    <Section id="why-us" className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Why teams choose us</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Built IELTS-native, not retrofitted
        </h2>
      </Reveal>

      <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {reasons.map((reason, i) => (
          <Reveal key={reason.title} delay={i * 0.06}>
            <GlassCard className="h-full">
              <reason.icon className="text-accent-violet" size={22} />
              <h3 className="mt-4 text-base font-medium">{reason.title}</h3>
              <p className="mt-2 text-sm text-muted">{reason.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
