import { Section } from "@/components/ui/Section";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";

const layers = [
  { title: "Edge", description: "Tenant is resolved from a validated JWT claim — never trusted from a request body or header." },
  { title: "Application", description: "No request executes without an established tenant context." },
  { title: "Domain", description: "Aggregates are tenant-scoped by design; cross-tenant references are impossible to construct." },
  { title: "Data", description: "A mandatory global query filter on tenant ID applies to every query, everywhere." },
  { title: "Cache, storage & jobs", description: "Cache keys, file paths, and background jobs are all tenant-prefixed." },
  { title: "Audit", description: "Every access is stamped with tenant, actor, and outcome in an immutable audit trail." },
];

export function IsolationLayers() {
  return (
    <Section id="isolation" className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Defense in depth</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Six layers, one rule: your tenant&apos;s data stays yours
        </h2>
        <p className="mt-4 max-w-2xl text-muted">
          A request for another tenant&apos;s resource returns a plain 404, never a 403 — so it&apos;s
          architecturally impossible to even confirm another tenant&apos;s data exists.
        </p>
      </Reveal>

      <div className="mt-12 mx-auto max-w-2xl">
        {layers.map((layer, i) => (
          <Reveal key={layer.title} delay={i * 0.05}>
            <div className="flex gap-4 border-l border-white/[0.08] py-4 pl-6 last:pb-0">
              <span className="font-mono text-xs text-accent-cyan">{String(i + 1).padStart(2, "0")}</span>
              <div>
                <h3 className="text-base font-medium">{layer.title}</h3>
                <p className="mt-1 text-sm text-muted">{layer.description}</p>
              </div>
            </div>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
