"use client";

import { Loader2, Mic, Star } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Progress } from "@/shared/components/ui/progress";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { Separator } from "@/shared/components/ui/separator";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import type { StudyCard } from "../types/vocabulary";
import { useReviewVocabulary } from "../hooks/use-review-vocabulary";
import { useMarkMastered } from "../hooks/use-mark-mastered";
import { MasteryBadge } from "./mastery-badge";
import { FillBlankForm } from "./fill-blank-form";

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

  const showMarkMastered = data.masteryStatus !== "New";

  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-2">
              <CardTitle className="text-2xl">{data.word}</CardTitle>
              <MasteryBadge status={data.masteryStatus} />
            </div>
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
        {/* Meanings */}
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

        <Separator />

        {/* Mastery Progress */}
        <div>
          <h3 className="mb-2 text-sm font-medium">Mastery Progress</h3>
          <div className="grid gap-3">
            <div>
              <div className="flex items-center justify-between text-xs mb-1">
                <span>Meaning</span>
                <span>{data.meaningMasteryScore}/100</span>
              </div>
              <Progress value={data.meaningMasteryScore} className="h-1.5" />
            </div>
            <div className="opacity-50">
              <div className="flex items-center justify-between text-xs mb-1">
                <span className="flex items-center gap-1">
                  Pronunciation
                  <span className="text-[10px] text-muted-foreground">(Coming soon)</span>
                </span>
                <span>&mdash;</span>
              </div>
              <Progress value={0} className="h-1.5" />
            </div>
            <div>
              <div className="flex items-center justify-between text-xs mb-1">
                <span>Example sentences</span>
                <span>{data.exampleSentenceScore}/100</span>
              </div>
              <Progress value={data.exampleSentenceScore} className="h-1.5" />
            </div>
          </div>
        </div>

        <Separator />

        {/* Review buttons */}
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
          {showMarkMastered && (
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger
                  render={
                    <Button
                      variant="secondary"
                      onClick={() => markMastered.mutate(data.vocabularyItemId)}
                      disabled={!data.canMarkMastered || markMastered.isPending}
                    />
                  }
                >
                  {markMastered.isPending && <Loader2 className="animate-spin" />}
                  <Star className="size-4" />
                  Mark mastered
                </TooltipTrigger>
                {!data.canMarkMastered && (
                  <TooltipContent>
                    Requires: 85+ score, 3+ reviews, 2+ correct streak
                  </TooltipContent>
                )}
              </Tooltip>
            </TooltipProvider>
          )}
        </div>

        {/* Examples with fill-in-the-blank */}
        {data.examples.length > 0 && (
          <>
            <Separator />
            <div>
              <h3 className="mb-2 text-sm font-medium">Example Sentences</h3>
              <div className="grid gap-2">
                {data.examples.map((example) =>
                  example.fillBlankSentence ? (
                    <FillBlankForm
                      key={example.exampleId}
                      example={example}
                      targetLanguageCode={targetLanguageCode!}
                    />
                  ) : (
                    <div
                      key={example.exampleId}
                      className="rounded-md bg-muted/50 px-3 py-2 text-sm italic text-muted-foreground"
                    >
                      {example.sentence}
                      {example.translations.length > 0 && (
                        <p className="mt-1 text-xs not-italic opacity-60">{example.translations[0]}</p>
                      )}
                    </div>
                  ),
                )}
              </div>
            </div>
          </>
        )}

        {/* Pronunciation coming soon */}
        <Separator />
        <div className="rounded-md border border-dashed p-4 opacity-60">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Mic className="size-4" />
            <span>AI pronunciation practice coming soon</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
