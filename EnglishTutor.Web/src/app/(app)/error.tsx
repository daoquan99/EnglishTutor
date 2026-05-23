"use client";

import { Button } from "@/shared/components/ui/button";

export default function AppError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="page-container page-section">
      <div className="py-12 text-center">
        <h1 className="text-2xl font-bold text-muted-foreground">
          Something went wrong
        </h1>
        <p className="mt-2 text-sm text-muted-foreground">
          {error.message || "An unexpected error occurred."}
        </p>
        <Button onClick={reset} className="mt-4">
          Try again
        </Button>
      </div>
    </div>
  );
}
