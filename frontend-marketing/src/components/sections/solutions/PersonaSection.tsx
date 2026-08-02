import { Section } from "@/components/ui/Section";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { cn } from "@/lib/utils";
import { Check, type LucideIcon } from "lucide-react";

export function PersonaSection({
  id,
  reverse = false,
  icon: Icon,
  badge,
  heading,
  description,
  points,
}: {
  id: string;
  reverse?: boolean;
  icon: LucideIcon;
  badge: string;
  heading: string;
  description: string;
  points: string[];
}) {
  return (
    <Section id={id} className="border-t border-white/[0.06]">
      <div className={cn("grid items-center gap-12 lg:grid-cols-2 lg:gap-16", reverse && "lg:[&>*:first-child]:order-2")}>
        <Reveal>
          <Badge>
            <Icon size={13} /> {badge}
          </Badge>
          <h2 className="mt-6 text-3xl font-semibold tracking-tight sm:text-4xl">{heading}</h2>
          <p className="mt-4 text-muted">{description}</p>
        </Reveal>

        <Reveal delay={0.1}>
          <ul className="space-y-4">
            {points.map((point) => (
              <li key={point} className="glass-panel flex items-start gap-3 rounded-md p-4 text-sm">
                <Check size={16} className="mt-0.5 shrink-0 text-accent-cyan" />
                <span className="text-muted">{point}</span>
              </li>
            ))}
          </ul>
        </Reveal>
      </div>
    </Section>
  );
}
