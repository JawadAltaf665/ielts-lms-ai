import type { Metadata } from "next";
import { Badge } from "@/components/ui/Badge";
import { Reveal } from "@/components/ui/Reveal";
import { Container } from "@/components/ui/Container";
import { Section } from "@/components/ui/Section";
import { LeadForm } from "@/components/forms/LeadForm";
import { CalendarClock } from "lucide-react";

export const metadata: Metadata = {
  title: "Book a Demo",
  description: "Book a demo of IELTS LMS AI for your institute, university, or enterprise program.",
};

export default function BookDemoPage() {
  return (
    <>
      <section className="relative overflow-hidden pt-40 pb-16 sm:pt-48">
        <div className="aurora-bg" aria-hidden />
        <Container className="relative z-10 max-w-3xl">
          <Reveal>
            <Badge>
              <CalendarClock size={13} /> Book a demo
            </Badge>
          </Reveal>
          <Reveal delay={0.08}>
            <h1 className="mt-6 text-4xl font-semibold tracking-tight sm:text-5xl">
              See it running on your program, not just a slide deck
            </h1>
          </Reveal>
          <Reveal delay={0.16}>
            <p className="mt-6 text-lg text-muted">
              Tell us a bit about your organization and we&apos;ll walk you through the platform,
              tailored to students, teachers, institutes, or enterprise programs.
            </p>
          </Reveal>
        </Container>
      </section>

      <Section className="border-t border-white/[0.06]">
        <div className="mx-auto max-w-xl">
          <Reveal>
            <LeadForm variant="demo" />
          </Reveal>
        </div>
      </Section>
    </>
  );
}
