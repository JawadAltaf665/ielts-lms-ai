import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";

const groups = [
  {
    title: "Learning & Coaching",
    agents: ["Grammar Agent", "Vocabulary Agent", "Sentence Builder Agent", "Reading Coach", "Listening Coach"],
  },
  {
    title: "Assessment & Prediction",
    agents: ["Writing Examiner Agent", "Speaking Examiner Agent", "Band Predictor Agent"],
  },
  {
    title: "Planning & Personalization",
    agents: ["Study Planner Agent", "Progress Analysis Agent", "Learning Recommendation Agent"],
  },
  {
    title: "Role & Operations",
    agents: ["Teacher Assistant", "Institute Assistant", "Support Agent", "Subscription Assistant"],
  },
];

export function AgentCatalog() {
  return (
    <Section id="agents" className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Agent catalog</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Four domains, purpose-built agents in each
        </h2>
      </Reveal>

      <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {groups.map((group, i) => (
          <Reveal key={group.title} delay={i * 0.06}>
            <GlassCard className="h-full">
              <h3 className="text-sm font-medium text-accent-cyan">{group.title}</h3>
              <ul className="mt-4 space-y-2.5">
                {group.agents.map((agent) => (
                  <li key={agent} className="text-sm text-muted">
                    {agent}
                  </li>
                ))}
              </ul>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
