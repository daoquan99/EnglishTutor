"use client";

import { useState } from "react";
import { Loader2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Progress } from "@/shared/components/ui/progress";
import type {
  ExerciseDetail,
  SubmitAnswerResponse,
} from "../types/exercises";
import { useSubmitAnswer } from "../hooks/use-submit-answer";
import { useCompleteAttempt } from "../hooks/use-complete-attempt";
import { QuestionRenderer } from "./question-renderer";
import { ExerciseResultView } from "./exercise-result-view";

interface ExercisePlayerProps {
  exercise: ExerciseDetail;
  attemptId: string;
}

export function ExercisePlayer({ exercise, attemptId }: ExercisePlayerProps) {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [feedbacks, setFeedbacks] = useState<
    Record<string, SubmitAnswerResponse>
  >({});

  const submitAnswer = useSubmitAnswer(attemptId);
  const completeAttempt = useCompleteAttempt(attemptId);

  const sorted = [...exercise.questions].sort((a, b) => a.order - b.order);
  const current = sorted[currentIndex];
  const totalAnswered = Object.keys(feedbacks).length;
  const allAnswered = totalAnswered === sorted.length;

  if (completeAttempt.data) {
    return <ExerciseResultView result={completeAttempt.data} />;
  }

  if (!current) return null;

  const handleSubmitAnswer = (userAnswer: string) => {
    submitAnswer.mutate(
      { questionId: current.id, userAnswer },
      {
        onSuccess: (resp) => {
          setFeedbacks((prev) => ({ ...prev, [current.id]: resp }));
        },
      },
    );
  };

  const handleNext = () => {
    if (currentIndex < sorted.length - 1) {
      setCurrentIndex((i) => i + 1);
    }
  };

  return (
    <div className="grid gap-6">
      <div className="flex items-center justify-between text-sm text-muted-foreground">
        <span>
          Question {currentIndex + 1} of {sorted.length}
        </span>
        <span>{totalAnswered} answered</span>
      </div>
      <Progress value={(totalAnswered / sorted.length) * 100} className="h-1.5" />

      <QuestionRenderer
        key={current.id}
        question={current}
        onSubmit={handleSubmitAnswer}
        isPending={submitAnswer.isPending}
        feedback={feedbacks[current.id] ?? null}
      />

      <div className="flex justify-between">
        <Button
          variant="outline"
          onClick={() => setCurrentIndex((i) => Math.max(0, i - 1))}
          disabled={currentIndex === 0}
        >
          Previous
        </Button>
        {allAnswered ? (
          <Button
            onClick={() => completeAttempt.mutate()}
            disabled={completeAttempt.isPending}
          >
            {completeAttempt.isPending && (
              <Loader2 className="animate-spin" />
            )}
            Finish exercise
          </Button>
        ) : (
          <Button
            onClick={handleNext}
            disabled={!feedbacks[current.id] || currentIndex === sorted.length - 1}
          >
            Next
          </Button>
        )}
      </div>
    </div>
  );
}
