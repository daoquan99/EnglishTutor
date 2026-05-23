"use client";

import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import type { ExerciseQuestion, SubmitAnswerResponse } from "../types/exercises";

interface QuestionRendererProps {
  question: ExerciseQuestion;
  onSubmit: (userAnswer: string) => void;
  isPending: boolean;
  feedback: SubmitAnswerResponse | null;
}

export function QuestionRenderer({
  question,
  onSubmit,
  isPending,
  feedback,
}: QuestionRendererProps) {
  const [selected, setSelected] = useState("");
  const [textAnswer, setTextAnswer] = useState("");
  const hasOptions = question.options.length > 0;
  const answered = !!feedback;

  const handleSubmit = () => {
    const answer = hasOptions ? selected : textAnswer.trim();
    if (!answer) return;
    onSubmit(answer);
  };

  return (
    <div className="grid gap-4">
      <p className="text-sm font-medium">{question.prompt}</p>

      {hasOptions ? (
        <div className="grid gap-2">
          {question.options
            .sort((a, b) => a.order - b.order)
            .map((opt) => (
              <button
                key={opt.id}
                type="button"
                disabled={answered}
                onClick={() => setSelected(opt.id)}
                className={`rounded-lg border p-3 text-left text-sm transition-colors ${
                  selected === opt.id
                    ? "border-primary bg-primary/10"
                    : "hover:bg-muted/50"
                } ${answered ? "cursor-default opacity-80" : "cursor-pointer"}`}
              >
                {opt.optionText}
              </button>
            ))}
        </div>
      ) : (
        <div className="grid gap-1.5">
          <Label htmlFor="answer">Your answer</Label>
          <Input
            id="answer"
            value={textAnswer}
            onChange={(e) => setTextAnswer(e.target.value)}
            placeholder="Type your answer..."
            disabled={answered}
            onKeyDown={(e) => e.key === "Enter" && handleSubmit()}
          />
        </div>
      )}

      {feedback && (
        <div
          className={`rounded-lg p-3 text-sm ${
            feedback.isCorrect
              ? "border-green-200 bg-green-50 dark:border-green-900 dark:bg-green-950"
              : "border-red-200 bg-red-50 dark:border-red-900 dark:bg-red-950"
          }`}
        >
          <p className="font-medium">
            {feedback.isCorrect ? "Correct!" : "Incorrect"} ({feedback.score}{" "}
            pts)
          </p>
          {feedback.feedback && (
            <p className="mt-1 text-muted-foreground">{feedback.feedback}</p>
          )}
          {feedback.explanation && (
            <p className="mt-1 text-muted-foreground">{feedback.explanation}</p>
          )}
        </div>
      )}

      {!answered && (
        <Button
          onClick={handleSubmit}
          disabled={isPending || (!selected && !textAnswer.trim())}
          className="w-fit"
        >
          Submit answer
        </Button>
      )}
    </div>
  );
}
