"use client";

import { useState } from "react";
import { BrainCircuit } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useExercises } from "@/features/exercises/hooks/use-exercises";
import { ExerciseList } from "@/features/exercises/components/exercise-list";
import { ExerciseFilters } from "@/features/exercises/components/exercise-filters";

export default function ExercisesPage() {
  const lang = useActiveLanguage();
  const [level, setLevel] = useState("all");
  const [skill, setSkill] = useState("all");

  const { data } = useExercises({
    targetLanguageCode: lang ?? undefined,
    level: level === "all" ? undefined : level,
    skill: skill === "all" ? undefined : skill,
  });

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={BrainCircuit}
        iconColor="bg-exercise/10 text-exercise"
        title="Exercises"
        description="Practice your language skills with interactive exercises."
      />

      <div className="mt-4">
        <ExerciseFilters
          level={level}
          skill={skill}
          onLevelChange={(v) => setLevel(v ?? "all")}
          onSkillChange={(v) => setSkill(v ?? "all")}
        />
      </div>

      <div className="mt-6">
        <ExerciseList exercises={data} />
      </div>
    </div>
  );
}
