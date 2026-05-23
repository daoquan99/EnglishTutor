"use client";

import { CheckCircle2, XCircle } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Progress } from "@/shared/components/ui/progress";
import { Separator } from "@/shared/components/ui/separator";
import type { ExerciseResult } from "../types/exercises";

function formatTime(seconds: number) {
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return `${m}m ${s}s`;
}

export function ExerciseResultView({ result }: { result: ExerciseResult }) {
  const pct = Math.round((result.correctCount / result.totalQuestions) * 100);

  return (
    <div className="grid gap-4">
      <Card>
        <CardHeader>
          <CardTitle>Exercise Complete</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-3">
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground">Score</span>
            <span className="font-medium">{result.totalScore} pts</span>
          </div>
          <Progress value={pct} className="h-2" />
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground">
              {result.correctCount}/{result.totalQuestions} correct
            </span>
            <span className="text-muted-foreground">
              {formatTime(result.timeTakenSeconds)}
            </span>
          </div>
        </CardContent>
      </Card>

      <Separator />

      <h3 className="text-sm font-medium">Answer Review</h3>
      <div className="grid gap-3">
        {result.answers.map((a) => (
          <div key={a.questionId} className="rounded-lg border p-3">
            <div className="flex items-start gap-2">
              {a.isCorrect ? (
                <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-green-600" />
              ) : (
                <XCircle className="mt-0.5 h-4 w-4 shrink-0 text-red-500" />
              )}
              <div className="grid gap-1 text-sm">
                <p>{a.prompt}</p>
                <p className="text-muted-foreground">
                  Your answer: {a.userAnswer}
                </p>
                {!a.isCorrect && a.correctAnswer && (
                  <p className="text-green-600 dark:text-green-400">
                    Correct: {a.correctAnswer}
                  </p>
                )}
                {a.explanation && (
                  <p className="text-xs text-muted-foreground">
                    {a.explanation}
                  </p>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
