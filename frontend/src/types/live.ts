export type CefrLevel = 'A1' | 'A2' | 'B1' | 'B2' | 'C1' | 'C2';

export interface DialogueTurnItem {
  turnId: string;
  speaker: 'user' | 'ai';
  /** Line 1: Gemini 3.5 Transcribe Live (larger font: 18px / --font-size-study-body) */
  transcriptLine1: string;
  isTranscriptFinal: boolean;
  /** Line 2: Gemini 3.5 Live Translate (smaller italic Vietnamese text only, no audio) */
  translationLine2: string;
  timestamp: string;
}

export interface LanguageCorrectionItem {
  turnId: string;
  originalText: string;
  correctedText: string;
  errorCategory: string;
  explanationVi: string;
  cefrTip: string;
  timestamp: string;
}
