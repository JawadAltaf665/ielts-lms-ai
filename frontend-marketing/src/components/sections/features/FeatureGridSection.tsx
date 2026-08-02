import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import type { LucideIcon } from "lucide-react";

export type FeatureGridItem = {
  icon: LucideIcon;
  title: string;
  description: string;
};

export function FeatureGridSection({
  id,
  eyebrow,
  heading,
  description,
  items,
}: {
  id?: string;
  eyebrow: string;
  heading: string;
  description?: string;
  items: FeatureGridItem[];
}) {
  return (
    <Section id={id} className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>{eyebrow}</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          {heading}
        </h2>
        {description && <p className="mt-4 max-w-2xl text-muted">{description}</p>}
      </Reveal>

      <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {items.map((item, i) => (
          <Reveal key={item.title} delay={(i % 3) * 0.06}>
            <GlassCard className="h-full">
              <item.icon className="text-accent-violet" size={22} />
              <h3 className="mt-4 text-lg font-medium">{item.title}</h3>
              <p className="mt-2 text-sm text-muted">{item.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
