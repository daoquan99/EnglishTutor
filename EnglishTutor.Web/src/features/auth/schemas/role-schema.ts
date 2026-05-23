import { z } from "zod";

export const roleFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Tên role bắt buộc")
    .max(100, "Tối đa 100 ký tự")
    .regex(
      /^[a-z0-9_-]+$/,
      "Chỉ chữ thường, số, dấu gạch ngang và underscore",
    ),
  description: z
    .string()
    .trim()
    .min(1, "Mô tả bắt buộc")
    .max(300, "Tối đa 300 ký tự"),
  isEnabled: z.boolean(),
  permissionIds: z.array(z.string().uuid()),
});

export type RoleFormValues = z.infer<typeof roleFormSchema>;
