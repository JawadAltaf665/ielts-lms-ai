import type { Metadata } from "next";
import { Badge } from "@/components/ui/Badge";
import { Reveal } from "@/components/ui/Reveal";
import { Container } from "@/components/ui/Container";
import { Section } from "@/components/ui/Section";
import { LeadForm } from "@/components/forms/LeadForm";
import { Mail } from "lucide-react";

export const metadata: Metadata = {
  title: "Contact",
  description: "Get in touch with the IELTS LMS AI team.",
};

export default function ContactPage() {
  return (
    <>
      <section className="relative overflow-hidden pt-40 pb-16 sm:pt-48">
        <div className="aurora-bg" aria-hidden />
        <Container className="relative z-10 max-w-3xl">
          <Reveal>
            <Badge>
              <Mail size={13} /> Contact
            </Badge>
          </Reveal>
          <Reveal delay={0.08}>
            <h1 className="mt-6 text-4xl font-semibold tracking-tight sm:text-5xl">
              Talk to the team
            </h1>
          </Reveal>
          <Reveal delay={0.16}>
            <p className="mt-6 text-lg text-muted">
              Questions about the platform, an institute rollout, or an enterprise contract —
              reach out and we&apos;ll route it to the right person.
            </p>
          </Reveal>
        </Container>
      </section>

      <Section className="border-t border-white/[0.06]">
        <div className="mx-auto max-w-xl">
          <Reveal>
            <LeadForm variant="contact" />
          </Reveal>
          <Reveal delay={0.06}>
            <p className="mt-6 text-center text-sm text-muted">
              Prefer email? Reach us at <span className="text-foreground">hello@ieltslms.ai</span>
            </p>
          </Reveal>
        </div>
      </Section>
    </>
  );
}
