"use client";

import { use } from "react";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useSession } from "@/features/speaking/hooks/use-session";
import { useSessionSummary } from "@/features/speaking/hooks/use-session-summary";
import { SessionChat } from "@/features/speaking/components/session-chat";

export default function SpeakingSessionPage({
  params,
}: {
  params: Promise<{ sessionId: string }>;
}) {
  const { sessionId } = use(params);
  const { data: session, isPending } = useSession(sessionId);
  const { data: summary } = useSessionSummary(
    sessionId,
    session?.status === "Completed",
  );

  return (
    <div className="page-container page-section">
      <Button variant="ghost" size="sm" render={<Link href="/speaking" />}>
        <ArrowLeft />
        Back to sessions
      </Button>

      <div className="mt-4">
        {isPending ? (
          <div className="grid gap-4">
            <Skeleton className="h-8 w-48" />
            <Skeleton className="h-32 w-full" />
          </div>
        ) : session ? (
          <SessionChat session={session} summary={summary} />
        ) : (
          <p className="text-sm text-muted-foreground">Session not found.</p>
        )}
      </div>
    </div>
  );
}
