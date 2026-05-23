import { httpClient } from "@/shared/api";
import type {
  ActivityLog,
  DashboardSnapshot,
  ExperienceInfo,
  MonthlyProgress,
  SkillProgress,
  WeeklyProgress,
} from "../types/progress";

export const progressApi = {
  getDashboard: (targetLanguageCode: string) =>
    httpClient.get<DashboardSnapshot>("/api/progress/dashboard/today", {
      params: { targetLanguageCode },
    }),

  getExperience: (targetLanguageCode: string) =>
    httpClient.get<ExperienceInfo>("/api/progress/experience", {
      params: { targetLanguageCode },
    }),

  getActivities: (targetLanguageCode: string) =>
    httpClient.get<ActivityLog[]>("/api/progress/activities", {
      params: { targetLanguageCode },
    }),

  getWeekly: (targetLanguageCode: string, year?: number, weekNumber?: number) =>
    httpClient.get<WeeklyProgress>("/api/progress/weekly", {
      params: { targetLanguageCode, year, weekNumber },
    }),

  getMonthly: (targetLanguageCode: string, year?: number, month?: number) =>
    httpClient.get<MonthlyProgress>("/api/progress/monthly", {
      params: { targetLanguageCode, year, month },
    }),

  getSkills: (targetLanguageCode: string) =>
    httpClient.get<SkillProgress[]>("/api/progress/skills", {
      params: { targetLanguageCode },
    }),
};
