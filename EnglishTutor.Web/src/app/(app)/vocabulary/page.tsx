"use client";

import { BookOpen } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useTodayVocabulary } from "@/features/vocabulary/hooks/use-today-vocabulary";
import { VocabularyList } from "@/features/vocabulary/components/vocabulary-list";

export default function VocabularyPage() {
  const lang = useActiveLanguage();
  const { data } = useTodayVocabulary(lang);

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={BookOpen}
        iconColor="bg-vocabulary/10 text-vocabulary"
        title="Vocabulary"
        description="Today's words to review and study."
      />
      <div className="mt-6">
        <VocabularyList items={data?.items} />
      </div>
    </div>
  );
}
