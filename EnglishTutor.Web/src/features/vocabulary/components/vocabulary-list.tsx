"use client";

import { useMemo } from "react";
import Link from "next/link";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/shared/components/ui/tabs";
import type { TodayVocabularyItem, MyVocabularyItem, MyVocabularyCounts } from "../types/vocabulary";
import { MasteryBadge } from "./mastery-badge";

interface VocabularyListProps {
  newItems?: TodayVocabularyItem[];
  reviewItems?: TodayVocabularyItem[];
  newWordsPerDay?: number;
  myWords?: MyVocabularyItem[];
  myCounts?: MyVocabularyCounts;
  isMyWordsLoading?: boolean;
}

export function VocabularyList({
  newItems,
  reviewItems,
  newWordsPerDay,
  myWords,
  myCounts,
  isMyWordsLoading,
}: VocabularyListProps) {
  const learningItems = useMemo(
    () =>
      myWords?.filter(
        (w) => w.masteryStatus === "Learning" || w.masteryStatus === "Reviewing" || w.masteryStatus === "Weak",
      ) ?? [],
    [myWords],
  );

  const masteredItems = useMemo(
    () => myWords?.filter((w) => w.masteryStatus === "Mastered") ?? [],
    [myWords],
  );

  const learningCount = (myCounts?.learning ?? 0) + (myCounts?.reviewing ?? 0) + (myCounts?.weak ?? 0);
  const masteredCount = myCounts?.mastered ?? 0;
  const newCount = newItems?.length ?? 0;
  const reviewCount = reviewItems?.length ?? 0;

  return (
    <Tabs defaultValue="new">
      <TabsList variant="line" className="w-full">
        <TabsTrigger value="new">
          New{newCount > 0 && ` (${newCount}/${newWordsPerDay ?? "?"})`}
        </TabsTrigger>
        <TabsTrigger value="learning">
          Learning{learningCount > 0 && ` (${learningCount})`}
        </TabsTrigger>
        <TabsTrigger value="mastered">
          Mastered{masteredCount > 0 && ` (${masteredCount})`}
        </TabsTrigger>
        <TabsTrigger value="review">
          Review{reviewCount > 0 && ` (${reviewCount})`}
        </TabsTrigger>
      </TabsList>

      <TabsContent value="new" className="mt-4">
        <ItemSection
          items={newItems}
          isLoading={!newItems && !reviewItems}
          emptyMessage="No new words for today."
        />
      </TabsContent>

      <TabsContent value="learning" className="mt-4">
        <ItemSection
          items={learningItems}
          isLoading={isMyWordsLoading}
          emptyMessage="No words in progress."
        />
      </TabsContent>

      <TabsContent value="mastered" className="mt-4">
        <ItemSection
          items={masteredItems}
          isLoading={isMyWordsLoading}
          emptyMessage="No mastered words yet. Keep studying!"
        />
      </TabsContent>

      <TabsContent value="review" className="mt-4">
        <ItemSection
          items={reviewItems}
          isLoading={!newItems && !reviewItems}
          emptyMessage="No words due for review."
        />
      </TabsContent>
    </Tabs>
  );
}

function ItemSection({
  items,
  isLoading,
  emptyMessage,
}: {
  items?: Array<TodayVocabularyItem | MyVocabularyItem>;
  isLoading?: boolean;
  emptyMessage: string;
}) {
  if (isLoading) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-14 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!items || items.length === 0) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        {emptyMessage}
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
            <div className="flex items-center gap-2">
              <span className="font-medium">{item.word}</span>
              {item.phonetic && (
                <span className="text-xs text-muted-foreground">{item.phonetic}</span>
              )}
            </div>
            <div className="flex items-center gap-2">
              {item.meaningMasteryScore > 0 && (
                <span className="text-xs text-muted-foreground">{item.meaningMasteryScore}%</span>
              )}
              <MasteryBadge status={item.masteryStatus} />
            </div>
          </Link>
        </li>
      ))}
    </ul>
  );
}
