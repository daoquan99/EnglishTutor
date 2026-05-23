"use client";

import { Loader2, Star } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Progress } from "@/shared/components/ui/progress";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { Separator } from "@/shared/components/ui/separator";
import type { StudyCard } from "../types/vocabulary";
import { useReviewVocabulary } from "../hooks/use-review-vocabulary";
import { useMarkMastered } from "../hooks/use-mark-mastered";

interface StudyCardDetailProps {
  data?: StudyCard;
  targetLanguageCode: string | null;
}

export function StudyCardDetail({ data, targetLanguageCode }: StudyCardDetailProps) {
  const review = useReviewVocabulary(targetLanguageCode);
  const markMastered = useMarkMastered(targetLanguageCode);

  if (!data) {
    return (
      <Card>
        <CardContent className="py-6">
          <div className="grid gap-4">
            <Skeleton className="h-8 w-48" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-20 w-full" />
          </div>
        </CardContent>
      </Card>
    );
  }

  const handleReview = (isCorrect: boolean) => {
    review.mutate({
      id: data.vocabularyItemId,
      data: { isCorrect, score: isCorrect ? 80 : 20 },
    });
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between">
          <div>
            <CardTitle className="text-2xl">{data.word}</CardTitle>
            {data.phonetic && (
              <p className="mt-1 text-sm text-muted-foreground">{data.phonetic}</p>
            )}
          </div>
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <span className="rounded-full bg-muted px-2 py-0.5">{data.partOfSpeech}</span>
            <span className="rounded-full bg-muted px-2 py-0.5">{data.topic}</span>
          </div>
        </div>
      </CardHeader>

      <CardContent className="grid gap-5">
        <div>
          <h3 className="mb-2 text-sm font-medium">Meanings</h3>
          <ul className="grid gap-1">
            {data.meanings.map((m, i) => (
              <li key={i} className="flex items-start gap-2 text-sm">
                <span className="mt-0.5 text-muted-foreground">{i + 1}.</span>
                {m}
              </li>
            ))}
          </ul>
        </div>

        {data.examples.length > 0 && (
          <div>
            <h3 className="mb-2 text-sm font-medium">Examples</h3>
            <ul className="grid gap-1.5">
              {data.examples.map((ex, i) => (
                <li
                  key={i}
                  className="rounded-md bg-muted/50 px-3 py-2 text-sm italic text-muted-foreground"
                >
                  {ex}
                </li>
              ))}
            </ul>
          </div>
        )}

        <Separator />

        <div>
          <h3 className="mb-2 text-sm font-medium">Mastery Progress</h3>
          <div className="grid gap-2">
            <div className="flex items-center justify-between text-xs">
              <span>Meaning</span>
              <span>{data.meaningMasteryScore}/100</span>
            </div>
            <Progress value={data.meaningMasteryScore} className="h-1.5" />
            <div className="flex items-center justify-between text-xs">
              <span>Pronunciation</span>
              <span>{data.pronunciationMasteryScore}/100</span>
            </div>
            <Progress value={data.pronunciationMasteryScore} className="h-1.5" />
            <div className="flex items-center justify-between text-xs">
              <span>Example sentences</span>
              <span>{data.exampleSentenceScore}/100</span>
            </div>
            <Progress value={data.exampleSentenceScore} className="h-1.5" />
          </div>
        </div>

        <Separator />

        <div className="flex flex-wrap gap-2">
          <Button
            variant="outline"
            onClick={() => handleReview(false)}
            disabled={review.isPending}
          >
            {review.isPending && !review.variables?.data.isCorrect && (
              <Loader2 className="animate-spin" />
            )}
            Didn&apos;t know
          </Button>
          <Button
            onClick={() => handleReview(true)}
            disabled={review.isPending}
          >
            {review.isPending && review.variables?.data.isCorrect && (
              <Loader2 className="animate-spin" />
            )}
            Got it!
          </Button>
          <Button
            variant="secondary"
            onClick={() => markMastered.mutate(data.vocabularyItemId)}
            disabled={markMastered.isPending}
          >
            {markMastered.isPending && <Loader2 className="animate-spin" />}
            <Star className="size-4" />
            Mark mastered
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
