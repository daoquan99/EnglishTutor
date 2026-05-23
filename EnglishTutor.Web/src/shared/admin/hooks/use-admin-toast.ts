"use client";

import { toast } from "sonner";
import { useCallback } from "react";
import { ApiError } from "@/shared/api/api-error";

export type UseAdminToastResult = {
  success: (message: string, options?: { description?: string }) => void;
  error: (error: unknown, fallbackMessage?: string) => void;
};

export function useAdminToast(): UseAdminToastResult {
  const success = useCallback(
    (message: string, options?: { description?: string }) => {
      toast.success(message, { description: options?.description });
    },
    [],
  );

  const error = useCallback((err: unknown, fallbackMessage = "Có lỗi xảy ra.") => {
    const { title, message } = extractApiErrorMessage(err, fallbackMessage);
    toast.error(title, { description: message });
  }, []);

  return { success, error };
}

export function extractApiErrorMessage(
  err: unknown,
  fallbackMessage = "Có lỗi xảy ra.",
): { title: string; message: string } {
  if (err instanceof ApiError) {
    if (err.isValidation && err.details) {
      const flat = Object.values(err.details).flat();
      return {
        title: err.message || "Dữ liệu không hợp lệ",
        message: flat.length > 0 ? flat.join(" · ") : err.message || fallbackMessage,
      };
    }
    return { title: err.message || fallbackMessage, message: err.message || fallbackMessage };
  }
  if (err instanceof Error) {
    return { title: err.message || fallbackMessage, message: err.message || fallbackMessage };
  }
  return { title: fallbackMessage, message: fallbackMessage };
}
