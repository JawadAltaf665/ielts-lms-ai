import { cn } from "@/lib/utils";

export function GlassCard({
  className,
  glow = false,
  children,
}: {
  className?: string;
  glow?: boolean;
  children: React.ReactNode;
}) {
  return (
    <div
      className={cn(
        "glass-panel relative rounded-lg p-6 transition-transform duration-300 ease-[var(--ease-out-expo)] hover:-translate-y-1",
        className
      )}
    >
      {glow && <div className="glow-border" aria-hidden />}
      <div className="relative z-10">{children}</div>
    </div>
  );
}
