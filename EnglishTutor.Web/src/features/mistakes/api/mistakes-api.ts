import { httpClient } from "@/shared/api";
import type { Mistake } from "../types/mistakes";

export const mistakesApi = {
  list: () => httpClient.get<Mistake[]>("/api/mistakes"),

  getToday: () => httpClient.get<Mistake[]>("/api/mistakes/today"),

  get: (id: string) => httpClient.get<Mistake>(`/api/mistakes/${id}`),

  review: (id: string) =>
    httpClient.post<Mistake>(`/api/mistakes/${id}/review`),

  markMastered: (id: string) =>
    httpClient.post<Mistake>(`/api/mistakes/${id}/mark-mastered`),
};
