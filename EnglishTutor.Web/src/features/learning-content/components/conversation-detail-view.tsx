"use client";

import { Badge } from "@/shared/components/ui/badge";
import { Separator } from "@/shared/components/ui/separator";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { ConversationDetail } from "../types/learning-content";

export function ConversationDetailView({
  conversation,
}: {
  conversation?: ConversationDetail;
}) {
  if (!conversation) {
    return (
      <div className="grid gap-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-60 w-full" />
      </div>
    );
  }

  const sorted = [...conversation.lines].sort((a, b) => a.order - b.order);

  return (
    <div className="grid gap-6">
      <div>
        <h1 className="text-2xl font-bold">{conversation.title}</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          {conversation.description}
        </p>
        <div className="mt-2 flex items-center gap-2">
          <Badge variant="secondary">{conversation.level}</Badge>
          <Badge variant="outline">{conversation.setting}</Badge>
          <span className="text-xs text-muted-foreground">
            ~{conversation.estimatedMinutes} min
          </span>
        </div>
      </div>

      <Separator />

      <div className="grid gap-3">
        {sorted.map((line) => (
          <div key={line.id} className="rounded-lg border p-3">
            <p className="text-xs font-medium text-muted-foreground">
              {line.speaker}
            </p>
            <p className="mt-1 text-sm">{line.text}</p>
            {line.expectedResponseHint && (
              <p className="mt-1 text-xs text-blue-600 dark:text-blue-400">
                Hint: {line.expectedResponseHint}
              </p>
            )}
            {line.notes && (
              <p className="mt-1 text-xs text-muted-foreground">
                {line.notes}
              </p>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
