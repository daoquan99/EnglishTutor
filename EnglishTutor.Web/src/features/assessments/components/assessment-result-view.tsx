"use client";

import { CheckCircle2, XCircle } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Progress } from "@/shared/components/ui/progress";
import { Separator } from "@/shared/components/ui/separator";
import type { AttemptResult } from "../types/assessments";
import { ATTEMPT_STATUS_LABELS } from "../types/assessments";

export function AssessmentResultView({ result }: { result: AttemptResult }) {
  return (
    <div className="grid gap-4">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            {result.isPassed ? (
              <CheckCircle2 className="h-5 w-5 text-green-600" />
            ) : (
              <XCircle className="h-5 w-5 text-red-500" />
            )}
            {result.isPassed ? "Passed!" : "Not passed"}
          </CardTitle>
        </CardHeader>
        <CardContent className="grid gap-3">
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground">Total score</span>
            <span className="font-medium">{result.totalScore}</span>
          </div>
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground">Status</span>
            <span>{ATTEMPT_STATUS_LABELS[result.status] ?? result.status}</span>
          </div>
        </CardContent>
      </Card>

      <Separator />

      <h3 className="text-sm font-medium">Skill Breakdown</h3>
      <div className="grid gap-3">
        {result.sectionScores.map((s) => (
          <div key={s.skill}>
            <div className="flex items-center justify-between text-sm">
              <span>{s.skill}</span>
              <span className="text-muted-foreground">{s.score}</span>
            </div>
            <Progress value={s.score} className="mt-1 h-1.5" />
          </div>
        ))}
      </div>
    </div>
  );
}
