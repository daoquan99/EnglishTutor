"use client";

import Link from "next/link";
import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { ConversationListItem } from "../types/learning-content";

export function ConversationList({
  conversations,
}: {
  conversations?: ConversationListItem[];
}) {
  if (!conversations) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 3 }, (_, i) => (
          <Skeleton key={i} className="h-20 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!conversations.length) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No conversations available.
      </p>
    );
  }

  return (
    <ul className="grid gap-2">
      {conversations.map((c) => (
        <li key={c.id}>
          <Link
            href={`/conversations/${c.id}`}
            className="flex items-center justify-between rounded-lg border p-4 transition-colors hover:bg-muted/50"
          >
            <div>
              <p className="font-medium">{c.title}</p>
              <p className="mt-0.5 text-xs text-muted-foreground">
                {c.setting} &middot; ~{c.estimatedMinutes} min
              </p>
            </div>
            <Badge variant="secondary">{c.level}</Badge>
          </Link>
        </li>
      ))}
    </ul>
  );
}
