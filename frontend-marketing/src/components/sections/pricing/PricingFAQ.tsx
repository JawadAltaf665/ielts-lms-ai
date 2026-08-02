import { Section } from "@/components/ui/Section";
import { Reveal } from "@/components/ui/Reveal";
import { Badge } from "@/components/ui/Badge";
import { ChevronDown } from "lucide-react";

const faqs = [
  {
    question: "How does the free trial work?",
    answer: "It's time-boxed and requires no payment upfront. One trial per organization to keep things fair, and it converts to a paid plan or simply expires — no surprise charges.",
  },
  {
    question: "How is payment processed?",
    answer: "Through Stripe. Your card details go directly to Stripe — we never see or store your full card number.",
  },
  {
    question: "Can I change plans later?",
    answer: "Yes, upgrades and downgrades are prorated against your next invoice.",
  },
  {
    question: "What happens if an institute exceeds its seat limit?",
    answer: "New seats are blocked until your admin increases the seat count, or the difference is prorated onto the next invoice — no silent overage charges.",
  },
];

export function PricingFAQ() {
  return (
    <Section className="border-t border-white/[0.06]">
      <Reveal>
        <Badge>Billing FAQ</Badge>
        <h2 className="mt-6 max-w-2xl text-3xl font-semibold tracking-tight sm:text-4xl">
          Billing questions, answered upfront
        </h2>
      </Reveal>

      <div className="mt-12 mx-auto max-w-3xl divide-y divide-white/[0.08]">
        {faqs.map((faq, i) => (
          <Reveal key={faq.question} delay={i * 0.04}>
            <details className="group py-5">
              <summary className="flex cursor-pointer list-none items-center justify-between gap-4 text-left text-base font-medium marker:content-none">
                {faq.question}
                <ChevronDown size={18} className="shrink-0 text-muted transition-transform duration-300 group-open:rotate-180" />
              </summary>
              <p className="mt-3 text-sm text-muted">{faq.answer}</p>
            </details>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
