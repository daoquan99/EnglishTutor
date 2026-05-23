"use client";

import { useState } from "react";
import { Loader2, Send, Square } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Progress } from "@/shared/components/ui/progress";
import { Separator } from "@/shared/components/ui/separator";
import type { SpeakingSession, SpeakingTurn, SessionSummary } from "../types/speaking";
import { useAddTurn } from "../hooks/use-add-turn";
import { useCompleteSession } from "../hooks/use-complete-session";

function CorrectionDisplay({ turn }: { turn: SpeakingTurn }) {
  const hasCorrection = turn.originalText !== turn.correctedText;
  return (
    <div className="rounded-lg border p-3">
      <div className="flex items-center gap-2 text-xs text-muted-foreground">
        <span>Grammar: {turn.grammarScore}</span>
        <span>&middot;</span>
        <span>Vocabulary: {turn.vocabularyScore}</span>
        <span>&middot;</span>
        <span>Overall: {turn.overallScore}</span>
      </div>
      <p className="mt-2 text-sm">
        <span className="font-medium">You said: </span>
        {turn.originalText}
      </p>
      {hasCorrection && (
        <p className="mt-1 text-sm text-green-600 dark:text-green-400">
          <span className="font-medium">Correction: </span>
          {turn.correctedText}
        </p>
      )}
      <p className="mt-1 text-sm text-blue-600 dark:text-blue-400">
        <span className="font-medium">Natural: </span>
        {turn.naturalVersion}
      </p>
      {turn.feedback && (
        <p className="mt-2 text-xs text-muted-foreground">{turn.feedback}</p>
      )}
    </div>
  );
}

function SummaryView({ summary }: { summary: SessionSummary }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Session Summary</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-3">
        <div className="grid grid-cols-2 gap-3 text-sm">
          <div>
            <span className="text-muted-foreground">Grammar</span>
            <Progress value={summary.averageGrammarScore} className="mt-1 h-1.5" />
          </div>
          <div>
            <span className="text-muted-foreground">Vocabulary</span>
            <Progress value={summary.averageVocabularyScore} className="mt-1 h-1.5" />
          </div>
          <div>
            <span className="text-muted-foreground">Pronunciation</span>
            <Progress value={summary.averagePronunciationScore} className="mt-1 h-1.5" />
          </div>
          <div>
            <span className="text-muted-foreground">Fluency</span>
            <Progress value={summary.averageFluencyScore} className="mt-1 h-1.5" />
          </div>
        </div>
        <Separator />
        <div className="text-sm">
          <p className="font-medium">Overall: {summary.overallScore}/100</p>
          <p className="text-muted-foreground">
            {summary.totalTurns} turns &middot; {summary.totalMistakes} mistakes
          </p>
        </div>
        {summary.strongPoints && (
          <p className="text-sm"><span className="font-medium">Strong: </span>{summary.strongPoints}</p>
        )}
        {summary.weakPoints && (
          <p className="text-sm"><span className="font-medium">Weak: </span>{summary.weakPoints}</p>
        )}
        {summary.recommendation && (
          <p className="text-sm"><span className="font-medium">Tip: </span>{summary.recommendation}</p>
        )}
      </CardContent>
    </Card>
  );
}

interface SessionChatProps {
  session: SpeakingSession;
  summary?: SessionSummary;
}

export function SessionChat({ session, summary }: SessionChatProps) {
  const [text, setText] = useState("");
  const [turns, setTurns] = useState<SpeakingTurn[]>([]);
  const addTurn = useAddTurn(session.sessionId);
  const completeSession = useCompleteSession(session.sessionId);

  const isActive = session.status === "Active";

  const handleSend = () => {
    if (!text.trim()) return;
    const msg = text.trim();
    setText("");
    addTurn.mutate(msg, {
      onSuccess: (turn) => setTurns((prev) => [...prev, turn]),
    });
  };

  if (session.status === "Completed" && summary) {
    return (
      <div className="grid gap-4">
        {turns.map((t) => (
          <CorrectionDisplay key={t.turnId} turn={t} />
        ))}
        <SummaryView summary={summary} />
      </div>
    );
  }

  return (
    <div className="grid gap-4">
      <div className="text-sm text-muted-foreground">
        {session.sessionType} &middot; {session.topic ?? "Free conversation"} &middot;{" "}
        {session.userLevel}
      </div>

      {turns.map((t) => (
        <CorrectionDisplay key={t.turnId} turn={t} />
      ))}

      {isActive && (
        <div className="flex gap-2">
          <Input
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder="Type your message..."
            onKeyDown={(e) => e.key === "Enter" && !e.shiftKey && handleSend()}
            disabled={addTurn.isPending}
          />
          <Button onClick={handleSend} disabled={!text.trim() || addTurn.isPending}>
            {addTurn.isPending ? <Loader2 className="animate-spin" /> : <Send />}
          </Button>
          <Button
            variant="secondary"
            onClick={() => completeSession.mutate()}
            disabled={completeSession.isPending || turns.length === 0}
          >
            {completeSession.isPending ? <Loader2 className="animate-spin" /> : <Square />}
            End
          </Button>
        </div>
      )}
    </div>
  );
}
