export interface DashboardSnapshot {
  userId: string;
  targetLanguageCode: string;
  date: string;
  totalExp: number;
  currentLevel: string;
  streakDays: number;
  vocabularyMastered: number;
  totalSpeakingSessions: number;
  totalExercisesCompleted: number;
  totalMistakes: number;
  weakSkills: string;
  strongSkills: string;
}

export interface ExperienceInfo {
  userId: string;
  targetLanguageCode: string;
  totalExp: number;
  currentAppRank: string;
}

export interface ActivityLog {
  activityId: string;
  activityType: string;
  completedAtUtc: string;
  durationSeconds: number;
  expEarned: number;
  score: number;
  result: string;
}

export interface WeeklyProgress {
  userId: string;
  targetLanguageCode: string;
  year: number;
  weekNumber: number;
  expEarned: number;
  activityCount: number;
}

export interface MonthlyProgress {
  userId: string;
  targetLanguageCode: string;
  year: number;
  month: number;
  expEarned: number;
  activityCount: number;
}

export interface SkillProgress {
  skill: string;
  score: number;
  activityCount: number;
  updatedAtUtc: string;
}

export const ACTIVITY_TYPE_LABELS: Record<string, string> = {
  LessonCompleted: "Lesson",
  VocabularyReviewed: "Vocabulary",
  VocabularyPronunciationPracticed: "Pronunciation",
  ExampleSentencePronunciationPracticed: "Sentence Practice",
  SpeakingSessionCompleted: "Speaking",
  ExerciseCompleted: "Exercise",
  MistakeReviewed: "Mistake Review",
  AssessmentCompleted: "Assessment",
  LevelUp: "Level Up",
  DailyGoalCompleted: "Daily Goal",
};

export const RANK_LABELS: Record<string, string> = {
  Beginner: "Beginner",
  Bronze: "Bronze",
  Silver: "Silver",
  Gold: "Gold",
  Platinum: "Platinum",
  Diamond: "Diamond",
};
