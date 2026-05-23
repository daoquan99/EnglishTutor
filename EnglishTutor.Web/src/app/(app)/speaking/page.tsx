"use client";

import { Mic } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useSessions } from "@/features/speaking/hooks/use-sessions";
import { SessionList } from "@/features/speaking/components/session-list";
import { StartSessionDialog } from "@/features/speaking/components/start-session-dialog";

export default function SpeakingPage() {
  const { data } = useSessions();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={Mic}
        iconColor="bg-speaking/10 text-speaking"
        title="Speaking Practice"
        description="Practice speaking with AI-powered feedback."
        action={<StartSessionDialog />}
      />
      <div className="mt-6">
        <SessionList sessions={data} />
      </div>
    </div>
  );
}
