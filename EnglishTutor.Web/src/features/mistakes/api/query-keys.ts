export const mistakeKeys = {
  all: ["mistakes"] as const,
  list: () => [...mistakeKeys.all, "list"] as const,
  today: () => [...mistakeKeys.all, "today"] as const,
  detail: (id: string) => [...mistakeKeys.all, "detail", id] as const,
};
