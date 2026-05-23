export interface UserOverviewCard {
  userId: string;
  email: string;
  displayName: string;
  targetLanguageCode: string;
  currentLevel: string;
  totalExp: number;
  currentStreakDays: number;
  totalSpeakingSessions: number;
  totalExercisesCompleted: number;
  totalVocabularyMastered: number;
  totalMistakes: number;
  lastActivityAtUtc: string | null;
  registeredAtUtc: string;
}

export interface DailyAiUsageReport {
  reportDate: string;
  modelType: string;
  taskType: string;
  totalRequests: number;
  totalPromptTokens: number;
  totalCompletionTokens: number;
  totalTokens: number;
  averageLatencyMs: number;
  failedRequests: number;
  estimatedCostUsd: number;
}

export interface LearningActivityReport {
  reportDate: string;
  period: string;
  totalActiveUsers: number;
  totalSpeakingSessions: number;
  totalExercisesCompleted: number;
  totalVocabularyReviews: number;
  totalLessonsCompleted: number;
  totalAssessments: number;
  totalStudyMinutes: number;
}

export interface CommonMistakeStat {
  targetLanguageCode: string;
  mistakeType: string;
  category: string;
  occurrenceCount: number;
  affectedUsers: number;
  exampleOriginal: string;
  exampleCorrected: string;
  lastUpdatedAtUtc: string;
}

export interface AssessmentPassRateReport {
  targetLanguageCode: string;
  assessmentType: string;
  forLevel: string;
  totalAttempts: number;
  passedCount: number;
  failedCount: number;
  passRate: number;
  averageScore: number;
  period: string;
  reportDate: string;
}

export interface AuditLog {
  id: string;
  adminUserId: string;
  action: string;
  targetEntity: string;
  targetEntityId: string;
  oldValue: string | null;
  newValue: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  createdAtUtc: string;
}

export interface DeadLetterMessage {
  id: string;
  eventId: string;
  eventType: string;
  sourceModule: string;
  failedAtUtc: string;
  retryCount: number;
  lastError: string;
  stackTrace: string | null;
  status: string;
}

export interface AiProvider {
  id: string;
  providerName: string;
  displayName: string;
  providerType: string;
  baseUrl: string | null;
  apiKeySecretName: string | null;
  isEnabled: boolean;
  models: AiProviderModel[];
}

export interface AiProviderModel {
  id: string;
  modelCode: string;
  displayName: string;
  capability: string;
  isEnabled: boolean;
  supportsStreaming: boolean;
  maxInputTokens: number;
  maxOutputTokens: number;
  costPerInput1KTokens: number;
  costPerOutput1KTokens: number;
  priority: number;
}

export interface RegisterProviderRequest {
  providerName: string;
  displayName: string;
  providerType: string;
  baseUrl?: string | null;
  apiKeySecretName?: string | null;
  isEnabled: boolean;
}

export interface UpsertModelRequest {
  modelCode: string;
  displayName: string;
  capability: string;
  supportsStreaming: boolean;
  maxInputTokens: number;
  maxOutputTokens: number;
  costPerInput1KTokens: number;
  costPerOutput1KTokens: number;
  priority: number;
  isEnabled: boolean;
}

export interface AiRuntimeRoute {
  id: string;
  taskType: string;
  capability: string;
  preferredProviderName: string;
  preferredModelCode: string;
  fallbackProviderName: string | null;
  fallbackModelCode: string | null;
  maxTokens: number;
  temperature: number;
  isActive: boolean;
}

export interface ConfigureRouteRequest {
  capability: string;
  preferredProviderName: string;
  preferredModelCode: string;
  fallbackProviderName?: string | null;
  fallbackModelCode?: string | null;
  maxTokens: number;
  temperature: number;
  isActive: boolean;
}
