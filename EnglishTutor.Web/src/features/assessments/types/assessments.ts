export interface AvailableAssessment {
  id: string;
  assessmentType: string;
  targetLanguageCode: string;
  forLevel: string;
  title: string;
  description: string | null;
  passingScore: number;
  minSkillScore: number;
  timeLimitMinutes: number | null;
}

export interface AttemptDetail {
  attemptId: string;
  assessmentDefinitionId: string;
  targetLanguageCode: string;
  currentLevel: string;
  status: string;
  sections: AssessmentSection[];
}

export interface AssessmentSection {
  id: string;
  skill: string;
  title: string;
  weight: number;
  order: number;
  questions: AssessmentQuestion[];
}

export interface AssessmentQuestion {
  id: string;
  prompt: string;
  questionType: string;
  isAiGraded: boolean;
  maxScore: number;
  order: number;
  explanation: string | null;
}

export interface SubmitAssessmentAnswersRequest {
  answers: AssessmentAnswerRequest[];
}

export interface AssessmentAnswerRequest {
  questionId: string;
  userAnswer: string;
}

export interface AttemptResult {
  attemptId: string;
  status: string;
  totalScore: number;
  isPassed: boolean;
  sectionScores: SectionScore[];
  gradedAtUtc: string | null;
}

export interface SectionScore {
  skill: string;
  score: number;
}

export interface StartLevelUpRequest {
  targetLanguageCode?: string | null;
}

export const ASSESSMENT_TYPES = [
  "PlacementTest",
  "LevelUpTest",
  "SkillCheck",
  "MonthlyReviewTest",
] as const;

export const ASSESSMENT_SKILLS = [
  "Grammar",
  "Vocabulary",
  "Reading",
  "Writing",
  "Speaking",
  "Listening",
] as const;

export const ATTEMPT_STATUS_LABELS: Record<string, string> = {
  InProgress: "In Progress",
  Submitted: "Submitted",
  Grading: "Grading",
  Passed: "Passed",
  Failed: "Failed",
};
