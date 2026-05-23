export interface ExerciseListItem {
  id: string;
  targetLanguageCode: string;
  level: string;
  topic: string;
  skill: string;
  exerciseType: string;
  title: string;
  description: string | null;
  totalQuestions: number;
}

export interface ExerciseDetail {
  id: string;
  targetLanguageCode: string;
  level: string;
  topic: string;
  skill: string;
  exerciseType: string;
  title: string;
  description: string | null;
  totalQuestions: number;
  questions: ExerciseQuestion[];
}

export interface ExerciseQuestion {
  id: string;
  questionType: string;
  prompt: string;
  explanation: string | null;
  order: number;
  difficulty: string;
  isAiGraded: boolean;
  options: QuestionOption[];
}

export interface QuestionOption {
  id: string;
  optionText: string;
  order: number;
}

export interface StartAttemptResponse {
  attemptId: string;
  exerciseSetId: string;
  targetLanguageCode: string;
  totalQuestions: number;
  startedAtUtc: string;
}

export interface SubmitAnswerRequest {
  questionId: string;
  userAnswer: string;
}

export interface SubmitAnswerResponse {
  attemptId: string;
  questionId: string;
  isCorrect: boolean;
  score: number;
  feedback: string | null;
  explanation: string | null;
  answeredAtUtc: string;
}

export interface ExerciseResult {
  attemptId: string;
  exerciseSetId: string;
  exerciseType: string;
  targetLanguageCode: string;
  totalScore: number;
  correctCount: number;
  totalQuestions: number;
  timeTakenSeconds: number;
  startedAtUtc: string;
  completedAtUtc: string;
  answers: AnswerResult[];
}

export interface AnswerResult {
  questionId: string;
  questionType: string;
  prompt: string;
  userAnswer: string;
  correctAnswer: string | null;
  isCorrect: boolean;
  score: number;
  feedback: string | null;
  explanation: string | null;
  answeredAtUtc: string;
}

export interface ExerciseListParams {
  page?: number;
  pageSize?: number;
  level?: string;
  type?: string;
  topic?: string;
  skill?: string;
  targetLanguageCode?: string;
}

export const EXERCISE_TYPES = [
  "MultipleChoice",
  "FillInTheBlank",
  "Matching",
  "Ordering",
  "FreeResponse",
] as const;

export const EXERCISE_SKILLS = [
  "Grammar",
  "Vocabulary",
  "Reading",
  "Writing",
  "Listening",
] as const;
