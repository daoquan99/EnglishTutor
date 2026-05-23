"use client";

import Link from "next/link";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { TodayVocabularyItem } from "../types/vocabulary";
import { MasteryBadge } from "./mastery-badge";

export function VocabularyList({ items }: { items?: TodayVocabularyItem[] }) {
  if (!items) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-14 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!items.length) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No vocabulary items for today. Check back later!
      </p>
    );
  }

  return (
    <ul className="grid gap-2">
      {items.map((item) => (
        <li key={item.vocabularyItemId}>
          <Link
            href={`/vocabulary/${item.vocabularyItemId}`}
            className="flex items-center justify-between rounded-lg border p-3 transition-colors hover:bg-muted/50"
          >
            <span className="font-medium">{item.word}</span>
            <MasteryBadge status={item.masteryStatus} />
          </Link>
        </li>
      ))}
    </ul>
  );
}
