"use client";

import { useState } from "react";
import { Check, X } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import type { StudyCardExample } from "../types/vocabulary";
import { useFillBlank } from "../hooks/use-fill-blank";
import type { FillBlankResult } from "../types/vocabulary";

interface FillBlankFormProps {
  example: StudyCardExample;
  targetLanguageCode: string;
}

export function FillBlankForm({ example, targetLanguageCode }: FillBlankFormProps) {
  const [answer, setAnswer] = useState("");
  const [result, setResult] = useState<FillBlankResult | null>(null);
  const fillBlank = useFillBlank(targetLanguageCode);

  const parts = example.fillBlankSentence?.split("_____") ?? [];

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!answer.trim() || result) return;

    fillBlank.mutate(
      { exampleId: example.exampleId, data: { userAnswer: answer.trim() } },
      {
        onSuccess: (data) => setResult(data),
      },
    );
  };

  return (
    <div className="rounded-md border p-3 space-y-2">
      <form onSubmit={handleSubmit} className="flex items-center gap-1 flex-wrap text-sm">
        {parts[0] && <span>{parts[0]}</span>}
        <Input
          value={answer}
          onChange={(e) => setAnswer(e.target.value)}
          disabled={!!result || fillBlank.isPending}
          className="w-32 h-7 text-sm inline-flex"
          placeholder="..."
        />
        {parts[1] && <span>{parts[1]}</span>}
        {!result && (
          <Button
            type="submit"
            size="sm"
            variant="outline"
            disabled={!answer.trim() || fillBlank.isPending}
            className="ml-2 h-7"
          >
            Check
          </Button>
        )}
      </form>

      {result && (
        <div className={`flex items-start gap-2 text-sm rounded-md px-2 py-1.5 ${result.isCorrect ? "bg-green-50 text-green-700 dark:bg-green-950/30 dark:text-green-400" : "bg-red-50 text-red-700 dark:bg-red-950/30 dark:text-red-400"}`}>
          {result.isCorrect ? <Check className="size-4 mt-0.5 shrink-0" /> : <X className="size-4 mt-0.5 shrink-0" />}
          <div>
            {result.isCorrect ? (
              <p>Correct!</p>
            ) : (
              <p>
                Incorrect. The answer is: <strong>{result.correctAnswer}</strong>
              </p>
            )}
            <p className="text-xs opacity-75 mt-0.5">{result.fullSentence}</p>
            {example.translations.length > 0 && (
              <p className="text-xs opacity-60 mt-0.5">{example.translations[0]}</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
