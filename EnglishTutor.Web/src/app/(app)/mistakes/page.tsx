"use client";

import { useState } from "react";
import { MessageSquare } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/ui/tabs";
import { PageHeader } from "@/shared/components/page-header";
import { useMistakes } from "@/features/mistakes/hooks/use-mistakes";
import { useTodayMistakes } from "@/features/mistakes/hooks/use-today-mistakes";
import { MistakeList } from "@/features/mistakes/components/mistake-list";

export default function MistakesPage() {
  const [tab, setTab] = useState("today");
  const { data: todayMistakes } = useTodayMistakes();
  const { data: allMistakes } = useMistakes();

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={MessageSquare}
        iconColor="bg-assessment/10 text-assessment"
        title="Mistakes"
        description="Review your mistakes and track your improvement."
      />

      <Tabs value={tab} onValueChange={setTab} className="mt-6">
        <TabsList>
          <TabsTrigger value="today">Today</TabsTrigger>
          <TabsTrigger value="all">All</TabsTrigger>
        </TabsList>
        <TabsContent value="today" className="mt-4">
          <MistakeList mistakes={todayMistakes} />
        </TabsContent>
        <TabsContent value="all" className="mt-4">
          <MistakeList mistakes={allMistakes} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
