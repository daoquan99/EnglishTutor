import { cn } from "@/shared/lib/utils";
import { MASTERY_STATUS_COLORS, MASTERY_STATUS_LABELS } from "../types/vocabulary";

export function MasteryBadge({ status, className }: { status: string; className?: string }) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium",
        MASTERY_STATUS_COLORS[status],
        className,
      )}
    >
      {MASTERY_STATUS_LABELS[status] ?? status}
    </span>
  );
}
