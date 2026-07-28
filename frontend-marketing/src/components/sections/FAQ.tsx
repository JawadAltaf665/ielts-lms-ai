import { Section } from "@/components/ui/Section";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { ChevronDown } from "lucide-react";

const faqs = [
  {
    question: "Are the AI band scores official IELTS results?",
    answer:
      "No. AI-generated scores and feedback are estimates for practice purposes, framed as advisory guidance — not official IELTS results. IELTS is a trademark of its respective owners, and we are not affiliated with or endorsed by them.",
  },
  {
    question: "How is my institute's data kept separate from other tenants?",
    answer:
      "Every tenant is isolated at every layer of the stack — application logic, database queries, caching, file storage, and background jobs are all scoped by tenant. Cross-tenant access attempts are architecturally blocked rather than merely permission-denied.",
  },
  {
    question: "What's included in the Free plan?",
    answer:
      "Limited grammar, vocabulary, reading, and listening practice, plus a trial AI writing evaluation. Full writing/speaking evaluation, mock exams, and band prediction are available on paid plans.",
  },
  {
    question: "Do you support single sign-on (SSO) for enterprise?",
    answer:
      "Enterprise SSO (SAML 2.0 / OIDC) is architected into the platform and is on our roadmap for enterprise rollout — reach out if this is a requirement for your evaluation timeline.",
  },
  {
    question: "Can institutes get dedicated data isolation?",
    answer:
      "Yes. Enterprise tenants can move from our shared-schema model to hybrid or fully dedicated database isolation as compliance requirements grow, without changing how the platform is used.",
  },
];

export function FAQ() {
  return (
    <Section id="faq" className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>FAQ</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Questions worth answering upfront
        </h2>
      </Reveal>

      <div className="mt-12 mx-auto max-w-3xl divide-y divide-white/[0.08]">
        {faqs.map((faq, i) => (
          <Reveal key={faq.question} delay={i * 0.04}>
            <details className="group py-5">
              <summary className="flex cursor-pointer list-none items-center justify-between gap-4 text-left text-base font-medium marker:content-none">
                {faq.question}
                <ChevronDown
                  size={18}
                  className="shrink-0 text-muted transition-transform duration-300 group-open:rotate-180"
                />
              </summary>
              <p className="mt-3 text-sm text-muted">{faq.answer}</p>
            </details>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
