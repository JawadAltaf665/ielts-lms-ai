import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";

export function LegalNotice() {
  return (
    <Section className="border-t border-white/[0.06]">
      <Reveal>
        <GlassCard className="max-w-3xl">
          <h3 className="text-sm font-medium">A note on trademarks and AI scores</h3>
          <p className="mt-3 text-sm text-muted">
            IELTS is a trademark of its respective owners. IELTS LMS AI is an independent
            preparation platform and does not claim official affiliation with or endorsement by
            the official test owners. AI-generated band scores and feedback are estimates intended
            for practice purposes and are not official IELTS results.
          </p>
        </GlassCard>
      </Reveal>
    </Section>
  );
}
