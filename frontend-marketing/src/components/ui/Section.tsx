import { cn } from "@/lib/utils";
import { Container } from "./Container";

export function Section({
  className,
  containerClassName,
  aurora = false,
  id,
  children,
}: {
  className?: string;
  containerClassName?: string;
  aurora?: boolean;
  id?: string;
  children: React.ReactNode;
}) {
  return (
    <section id={id} className={cn("relative py-24 sm:py-32 overflow-hidden", className)}>
      {aurora && <div className="aurora-bg" aria-hidden />}
      <Container className={cn("relative z-10", containerClassName)}>{children}</Container>
    </section>
  );
}
