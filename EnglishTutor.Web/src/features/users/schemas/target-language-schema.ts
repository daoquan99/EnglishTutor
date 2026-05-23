import { z } from "zod";
import { LANGUAGE_LEVELS } from "../types/users";

export const addTargetLanguageSchema = z.object({
  targetLanguageCode: z.string().min(2, "Required").max(3),
  currentLevel: z.enum(LANGUAGE_LEVELS, { message: "Select a level" }),
  targetLevel: z.enum(LANGUAGE_LEVELS, { message: "Select a level" }),
});

export type AddTargetLanguageFormValues = z.infer<typeof addTargetLanguageSchema>;
