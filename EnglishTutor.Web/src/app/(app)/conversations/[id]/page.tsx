"use client";

import { use } from "react";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { useConversationDetail } from "@/features/learning-content/hooks/use-conversation-detail";
import { ConversationDetailView } from "@/features/learning-content/components/conversation-detail-view";

export default function ConversationDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: conversation } = useConversationDetail(id);

  return (
    <div className="page-container page-section">
      <Button variant="ghost" size="sm" render={<Link href="/conversations" />}>
        <ArrowLeft />
        Back to conversations
      </Button>
      <div className="mt-4">
        <ConversationDetailView conversation={conversation} />
      </div>
    </div>
  );
}
