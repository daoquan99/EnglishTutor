"use client";

import { useState } from "react";
import { Loader2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Progress } from "@/shared/components/ui/progress";
import { Separator } from "@/shared/components/ui/separator";
import type { AttemptDetail } from "../types/assessments";
import { useSubmitAssessmentAnswers } from "../hooks/use-submit-assessment-answers";
import { useSubmitAssessment } from "../hooks/use-submit-assessment";
import { AssessmentResultView } from "./assessment-result-view";

interface AssessmentPlayerProps {
  attempt: AttemptDetail;
}

export function AssessmentPlayer({ attempt }: AssessmentPlayerProps) {
  const allQuestions = attempt.sections
    .sort((a, b) => a.order - b.order)
    .flatMap((s) =>
      s.questions
        .sort((a, b) => a.order - b.order)
        .map((q) => ({ ...q, sectionTitle: s.title, skill: s.skill })),
    );

  const [currentIndex, setCurrentIndex] = useState(0);
  const [answers, setAnswers] = useState<Record<string, string>>({});

  const submitAnswers = useSubmitAssessmentAnswers(attempt.attemptId);
  const submitAssessment = useSubmitAssessment(attempt.attemptId);

  if (submitAssessment.data) {
    return <AssessmentResultView result={submitAssessment.data} />;
  }

  const current = allQuestions[currentIndex];
  if (!current) return null;

  const totalAnswered = Object.keys(answers).length;

  const handleSaveAnswer = () => {
    const answer = answers[current.id];
    if (!answer?.trim()) return;
    submitAnswers.mutate({
      answers: [{ questionId: current.id, userAnswer: answer }],
    });
    if (currentIndex < allQuestions.length - 1) {
      setCurrentIndex((i) => i + 1);
    }
  };

  const handleFinish = () => {
    submitAssessment.mutate();
  };

  return (
    <div className="grid gap-6">
      <div className="flex items-center justify-between text-sm text-muted-foreground">
        <span>
          {current.skill}: {current.sectionTitle}
        </span>
        <span>
          {currentIndex + 1} / {allQuestions.length}
        </span>
      </div>
      <Progress
        value={(totalAnswered / allQuestions.length) * 100}
        className="h-1.5"
      />

      <Separator />

      <div className="grid gap-4">
        <p className="text-sm font-medium">{current.prompt}</p>
        <div className="grid gap-1.5">
          <Label htmlFor="assessment-answer">Your answer</Label>
          <Input
            id="assessment-answer"
            value={answers[current.id] ?? ""}
            onChange={(e) =>
              setAnswers((prev) => ({ ...prev, [current.id]: e.target.value }))
            }
            placeholder="Type your answer..."
            onKeyDown={(e) => e.key === "Enter" && handleSaveAnswer()}
          />
        </div>
      </div>

      <div className="flex justify-between">
        <Button
          variant="outline"
          onClick={() => setCurrentIndex((i) => Math.max(0, i - 1))}
          disabled={currentIndex === 0}
        >
          Previous
        </Button>
        <div className="flex gap-2">
          {currentIndex < allQuestions.length - 1 ? (
            <Button
              onClick={handleSaveAnswer}
              disabled={!answers[current.id]?.trim() || submitAnswers.isPending}
            >
              {submitAnswers.isPending && <Loader2 className="animate-spin" />}
              Next
            </Button>
          ) : (
            <Button
              onClick={handleFinish}
              disabled={
                totalAnswered < allQuestions.length ||
                submitAssessment.isPending
              }
            >
              {submitAssessment.isPending && (
                <Loader2 className="animate-spin" />
              )}
              Submit assessment
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
