"use client";

import { Loader2 } from "lucide-react";
import { Checkbox } from "@/shared/components/ui/checkbox";
import type { WeekDay } from "../types/study-plans";
import { DAY_LABELS } from "../types/study-plans";
import { useUpdateSchedule } from "../hooks/use-update-schedule";

interface WeekScheduleEditorProps {
  weekDays: WeekDay[];
  targetLanguageCode: string;
}

export function WeekScheduleEditor({
  weekDays,
  targetLanguageCode,
}: WeekScheduleEditorProps) {
  const updateSchedule = useUpdateSchedule(targetLanguageCode);

  const toggle = (dayOfWeek: number) => {
    const updated = weekDays.map((d) =>
      d.dayOfWeek === dayOfWeek ? { ...d, isStudyDay: !d.isStudyDay } : d,
    );
    updateSchedule.mutate({ days: updated });
  };

  return (
    <div className="grid gap-2">
      {weekDays
        .sort((a, b) => a.dayOfWeek - b.dayOfWeek)
        .map((d) => (
          <label
            key={d.dayOfWeek}
            className="flex cursor-pointer items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-muted/50"
          >
            <Checkbox
              checked={d.isStudyDay}
              onCheckedChange={() => toggle(d.dayOfWeek)}
            />
            <span className="text-sm">{DAY_LABELS[d.dayOfWeek]}</span>
          </label>
        ))}
      {updateSchedule.isPending && (
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <Loader2 className="h-3 w-3 animate-spin" />
          Saving...
        </div>
      )}
    </div>
  );
}
