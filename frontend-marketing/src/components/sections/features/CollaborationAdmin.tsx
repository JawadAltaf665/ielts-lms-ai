import { FeatureGridSection } from "./FeatureGridSection";
import { Presentation, MessageSquare, Bell, FileBarChart2 } from "lucide-react";

const items = [
  { icon: Presentation, title: "Teacher Portal", description: "Assignments, grading oversight, and class-level visibility in one dashboard." },
  { icon: MessageSquare, title: "Messaging", description: "Direct, tenant-scoped communication between teacher and student." },
  { icon: Bell, title: "Notifications", description: "Reminders and plan updates that matter, not noise." },
  { icon: FileBarChart2, title: "Institute Reports", description: "Institute-wide reporting and settings from the admin portal." },
];

export function CollaborationAdmin() {
  return (
    <FeatureGridSection
      id="collaboration"
      eyebrow="For teachers & institutes"
      heading="Built for classrooms, not just individuals"
      items={items}
    />
  );
}
