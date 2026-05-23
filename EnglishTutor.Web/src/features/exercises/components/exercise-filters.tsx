"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { EXERCISE_SKILLS } from "../types/exercises";

const LEVELS = ["A1", "A2", "B1", "B2", "C1", "C2"];

interface ExerciseFiltersProps {
  level: string;
  skill: string;
  onLevelChange: (v: string | null) => void;
  onSkillChange: (v: string | null) => void;
}

export function ExerciseFilters({
  level,
  skill,
  onLevelChange,
  onSkillChange,
}: ExerciseFiltersProps) {
  return (
    <div className="flex flex-wrap gap-3">
      <Select value={level} onValueChange={onLevelChange}>
        <SelectTrigger className="w-32">
          <SelectValue placeholder="Level" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All levels</SelectItem>
          {LEVELS.map((l) => (
            <SelectItem key={l} value={l}>
              {l}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select value={skill} onValueChange={onSkillChange}>
        <SelectTrigger className="w-36">
          <SelectValue placeholder="Skill" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All skills</SelectItem>
          {EXERCISE_SKILLS.map((s) => (
            <SelectItem key={s} value={s}>
              {s}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
