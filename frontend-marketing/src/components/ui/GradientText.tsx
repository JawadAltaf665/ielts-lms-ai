import { createElement } from "react";
import { cn } from "@/lib/utils";

export function GradientText({
  className,
  as = "span",
  children,
}: {
  className?: string;
  as?: keyof React.JSX.IntrinsicElements;
  children: React.ReactNode;
}) {
  return createElement(as, { className: cn("text-gradient", className) }, children);
}
