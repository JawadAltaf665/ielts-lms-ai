import { FeatureGridSection } from "./FeatureGridSection";
import { CalendarClock, CalendarCheck, BarChart3, Trophy, Award } from "lucide-react";

const items = [
  { icon: CalendarClock, title: "Daily Study Planner", description: "An AI-adjusted plan based on real performance, generated fresh every day." },
  { icon: CalendarCheck, title: "Weekly Assessments", description: "Regular checkpoints between monthly mock exams to catch drift early." },
  { icon: BarChart3, title: "Progress Tracking & Analytics", description: "See exactly which skills are moving your estimated band score." },
  { icon: Trophy, title: "Achievements", description: "Milestones that keep momentum going, without gimmicks." },
  { icon: Award, title: "Certificates", description: "Verifiable completion certificates with unique, checkable IDs." },
];

export function PlanningProgress() {
  return (
    <FeatureGridSection
      id="planning"
      eyebrow="Planning & progress"
      heading="Always know if you're on track"
      items={items}
    />
  );
}
