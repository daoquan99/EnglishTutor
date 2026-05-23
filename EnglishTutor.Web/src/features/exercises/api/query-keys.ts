import type { ExerciseListParams } from "../types/exercises";

export const exerciseKeys = {
  all: ["exercises"] as const,
  list: (params: ExerciseListParams) => [...exerciseKeys.all, "list", params] as const,
  detail: (id: string) => [...exerciseKeys.all, "detail", id] as const,
  result: (attemptId: string) => [...exerciseKeys.all, "result", attemptId] as const,
};
