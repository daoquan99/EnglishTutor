"use client";

import { Server } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useAiProviders } from "@/features/admin-reports/hooks/use-admin-ai";
import { AiProvidersTable } from "@/features/admin-reports/components/ai-providers-table";

export default function AiProvidersPage() {
  const { data } = useAiProviders();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={Server}
        iconColor="bg-assessment/10 text-assessment"
        title="AI Providers"
        description="Manage AI provider configurations."
      />
      <div className="mt-6">
        <AiProvidersTable providers={data} />
      </div>
    </div>
  );
}
