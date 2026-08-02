import { FeatureGridSection } from "./FeatureGridSection";
import { SpellCheck, BookMarked, Layers, BookOpen, Headphones, Route, GraduationCap, ClipboardList } from "lucide-react";

const items = [
  { icon: SpellCheck, title: "Grammar", description: "Targeted drills that adapt to the mistakes you actually make." },
  { icon: BookMarked, title: "Vocabulary", description: "Spaced, contextual vocabulary building around real exam topics." },
  { icon: Layers, title: "Sentence Builder", description: "Structured practice for constructing accurate, natural sentences." },
  { icon: BookOpen, title: "Reading", description: "Timed passages covering every real IELTS question type." },
  { icon: Headphones, title: "Listening", description: "Full-length audio sections replicating real exam conditions." },
  { icon: Route, title: "Learning Paths", description: "Personalized, adaptive sequencing instead of a fixed syllabus." },
  { icon: GraduationCap, title: "Courses", description: "Structured curriculum content organized by skill and level." },
  { icon: ClipboardList, title: "Practice & Assignments", description: "Free practice plus teacher-assigned work, in one place." },
];

export function LearningPractice() {
  return (
    <FeatureGridSection
      id="learning"
      eyebrow="Learning & practice"
      heading="Practice that adapts to you, skill by skill"
      items={items}
    />
  );
}
