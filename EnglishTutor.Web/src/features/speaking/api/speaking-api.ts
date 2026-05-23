import { httpClient } from "@/shared/api";
import type {
  SessionSummary,
  SpeakingSession,
  SpeakingTurn,
  StartSessionRequest,
} from "../types/speaking";

export const speakingApi = {
  listSessions: (page = 1, pageSize = 20) =>
    httpClient.get<SpeakingSession[]>("/api/speaking/sessions", {
      params: { page, pageSize },
    }),

  getSession: (id: string) =>
    httpClient.get<SpeakingSession>(`/api/speaking/sessions/${id}`),

  startSession: (data: StartSessionRequest) =>
    httpClient.post<SpeakingSession>("/api/speaking/sessions", data),

  addTextTurn: (sessionId: string, userText: string) =>
    httpClient.post<SpeakingTurn>(`/api/speaking/sessions/${sessionId}/turns`, {
      userText,
    }),

  addAudioTurn: (sessionId: string, audioFile: File, userText?: string) => {
    const formData = new FormData();
    formData.append("AudioFile", audioFile);
    if (userText) formData.append("UserText", userText);
    return httpClient.postFormData<SpeakingTurn>(
      `/api/speaking/sessions/${sessionId}/turns/audio`,
      formData,
    );
  },

  completeSession: (id: string) =>
    httpClient.post<SessionSummary>(`/api/speaking/sessions/${id}/complete`),

  getSummary: (id: string) =>
    httpClient.get<SessionSummary>(`/api/speaking/sessions/${id}/summary`),
};
