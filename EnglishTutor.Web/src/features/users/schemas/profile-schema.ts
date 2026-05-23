import { z } from "zod";

export const profileSchema = z.object({
  displayName: z
    .string()
    .min(2, "Display name must be at least 2 characters")
    .max(100, "Display name must be at most 100 characters"),
  avatarUrl: z.string().max(2048).nullable().optional(),
  bio: z.string().max(500, "Bio must be at most 500 characters").nullable().optional(),
});

export type ProfileFormValues = z.infer<typeof profileSchema>;
