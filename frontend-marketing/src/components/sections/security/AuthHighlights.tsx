import { Section } from "@/components/ui/Section";
import { GlassCard } from "@/components/ui/GlassCard";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { KeyRound, Lock, RotateCcw, Vault } from "lucide-react";

const points = [
  {
    icon: KeyRound,
    title: "RBAC, deny by default",
    description: "Role- and permission-based authorization, always tenant-scoped. Nothing is allowed unless explicitly granted.",
  },
  {
    icon: RotateCcw,
    title: "Refresh token reuse detection",
    description: "A stolen or replayed refresh token triggers full token-family revocation and a security alert — automatic breach containment.",
  },
  {
    icon: Lock,
    title: "Encryption everywhere",
    description: "TLS in transit, AES-256-class encryption at rest, with field-level encryption for the most sensitive data.",
  },
  {
    icon: Vault,
    title: "Vault-managed secrets",
    description: "Signing keys, credentials, and certificates are never in code or logs, and rotate on a schedule.",
  },
];

export function AuthHighlights() {
  return (
    <Section className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Identity & encryption</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Enterprise-grade IAM and encryption architecture
        </h2>
      </Reveal>

      <div className="mt-14 grid gap-6 sm:grid-cols-2">
        {points.map((point, i) => (
          <Reveal key={point.title} delay={i * 0.06}>
            <GlassCard className="h-full">
              <point.icon className="text-accent-violet" size={22} />
              <h3 className="mt-4 text-base font-medium">{point.title}</h3>
              <p className="mt-2 text-sm text-muted">{point.description}</p>
            </GlassCard>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
