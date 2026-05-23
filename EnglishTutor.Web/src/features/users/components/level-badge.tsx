import { cn } from "@/shared/lib/utils";

const levelColors: Record<string, string> = {
  A1: "text-level-a1",
  A2: "text-level-a2",
  B1: "text-level-b1",
  B2: "text-level-b2",
  C1: "text-level-c1",
  C2: "text-level-c2",
};

interface LevelBadgeProps {
  level: string;
  className?: string;
}

export function LevelBadge({ level, className }: LevelBadgeProps) {
  const upper = level.toUpperCase();
  return (
    <span className={cn("level-pill", levelColors[upper], className)}>
      {upper}
    </span>
  );
}
