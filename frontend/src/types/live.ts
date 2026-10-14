export type CefrLevel = 'A1' | 'A2' | 'B1' | 'B2' | 'C1' | 'C2';

export type TeacherVoiceName = 'Aoede' | 'Puck' | 'Kore' | 'Charon' | 'Fenrir' | 'Leda';

export type ConversationModeId =
  // 12 Interview & Coaching Modes
  | 'practice_interview'
  | 'real_interview'
  | 'hr_interview'
  | 'technical_interview'
  | 'system_design'
  | 'behavioral_star'
  | 'stress_interview'
  | 'rapid_fire'
  | 'deep_dive'
  | 'english_coach'
  | 'pronunciation_coach'
  | 'resume_deep_dive'
  // 7 Communication & Workplace Modes
  | 'daily_conversation'
  | 'daily_standup'
  | 'technical_discussion'
  | 'problem_solving'
  | 'client_meeting'
  | 'debate_mode'
  | 'roleplay_mode';

export interface ConversationModeOption {
  id: ConversationModeId;
  group: 'interview' | 'communication';
  name: string;
  descVi: string;
  suggestedTopics: string[];
}

export interface ConversationSummary {
  id: string;
  title: string;
  cefrLevel: CefrLevel;
  extendedThinking: boolean;
  voiceName: TeacherVoiceName;
  speechSpeed: number;
  conversationMode: ConversationModeId;
  topic: string;
  customSystemPrompt?: string | null;
  createdAt: string;
  updatedAt: string;
  messageCount: number;
  lastMessagePreview: string;
}

export interface DialogueTurnItem {
  turnId: string;
  speaker: 'user' | 'ai';
  sequenceOrder: number;
  /** Line 1: Gemini 3.5 Transcribe Live (larger font: 18px / --font-size-study-body) */
  transcriptLine1: string;
  isTranscriptFinal: boolean;
  /** Line 2: Gemini 3.5 Live Translate (smaller italic Vietnamese text only, no audio) */
  translationLine2: string;
  timestamp: string;
}

export interface LanguageCorrectionItem {
  id?: string;
  turnId: string;
  originalText: string;
  correctedText: string;
  errorCategory: string;
  explanationVi: string;
  cefrTip: string;
  timestamp: string;
}
