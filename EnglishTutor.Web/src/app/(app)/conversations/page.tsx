"use client";

import { MessageSquare } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useConversations } from "@/features/learning-content/hooks/use-conversations";
import { ConversationList } from "@/features/learning-content/components/conversation-list";

export default function ConversationsPage() {
  const lang = useActiveLanguage();
  const { data } = useConversations({ targetLanguageCode: lang ?? undefined });

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={MessageSquare}
        iconColor="bg-speaking/10 text-speaking"
        title="Conversations"
        description="Practice with guided conversation scenarios."
      />
      <div className="mt-6">
        <ConversationList conversations={data} />
      </div>
    </div>
  );
}
