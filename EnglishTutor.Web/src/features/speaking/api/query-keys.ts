export const speakingKeys = {
  all: ["speaking"] as const,
  sessions: () => [...speakingKeys.all, "sessions"] as const,
  session: (id: string) => [...speakingKeys.all, "session", id] as const,
  summary: (id: string) => [...speakingKeys.all, "summary", id] as const,
};
