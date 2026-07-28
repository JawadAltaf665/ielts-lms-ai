import type { Metadata } from "next";
import { GraduationCap, Presentation, Building2, Building } from "lucide-react";
import { SolutionsHero } from "@/components/sections/solutions/SolutionsHero";
import { PersonaSection } from "@/components/sections/solutions/PersonaSection";
import { CTA } from "@/components/sections/CTA";

export const metadata: Metadata = {
  title: "Solutions",
  description:
    "How IELTS LMS AI works for students, teachers, institutes, and enterprise organizations — one platform, built for every seat at the table.",
};

export default function SolutionsPage() {
  return (
    <>
      <SolutionsHero />
      <PersonaSection
        id="students"
        icon={GraduationCap}
        badge="For students"
        heading="Reach your target band without the guesswork"
        description="Built for learners who need a realistic path to a specific band by a specific date — not another generic study app."
        points={[
          "AI-scored writing and speaking feedback in place of expensive private tutoring",
          "A full computer-based mock exam, so exam day isn't the first time you see the interface",
          "One daily plan instead of fragmented resources across five different apps",
          "A confidence-scored band prediction instead of guessing where you stand",
        ]}
      />
      <PersonaSection
        id="teachers"
        reverse
        icon={Presentation}
        badge="For teachers"
        heading="AI handles the first pass, you handle what matters"
        description="A coaching layer under your classroom, not a replacement for it — built to give time back, not take it away."
        points={[
          "AI does first-pass grading on writing and speaking so you focus on the students who need you",
          "Class-level analytics instead of guessing who's falling behind",
          "Assignments and student messaging in one dashboard",
          "You stay the teacher of record — AI output is advisory, not final, by default",
        ]}
      />
      <PersonaSection
        id="institutes"
        icon={Building2}
        badge="For institutes"
        heading="Run your program without building the software yourself"
        description="One dashboard for every teacher and student in your program, instead of admin work spread across spreadsheets and email."
        points={[
          "Institute-wide analytics instead of piecing together spreadsheets",
          "Seat-based billing with a single consolidated invoice",
          "Teacher and cohort tools built in, not bolted on",
          "Onboard an entire cohort in the time it takes to onboard one student elsewhere",
        ]}
      />
      <PersonaSection
        id="enterprise"
        reverse
        icon={Building}
        badge="For enterprise"
        heading="Infrastructure your compliance team will actually sign off on"
        description="Tenant isolation architected at every layer from day one, with a clear path to stricter guarantees as you grow."
        points={[
          "Skip building IELTS prep technology in-house entirely",
          "Cross-tenant access is architecturally blocked, not just permission-denied",
          "Dedicated database isolation available on request as compliance needs grow",
          "Custom contracts, SLAs, and enterprise SSO (SAML/OIDC) on our roadmap",
        ]}
      />
      <CTA />
    </>
  );
}
