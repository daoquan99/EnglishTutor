"use client";

import { BookOpen } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useTodayVocabulary } from "@/features/vocabulary/hooks/use-today-vocabulary";
import { useMyVocabulary } from "@/features/vocabulary/hooks/use-my-vocabulary";
import { VocabularyList } from "@/features/vocabulary/components/vocabulary-list";
import { StudySettingsDialog } from "@/features/vocabulary/components/study-settings-dialog";

export default function VocabularyPage() {
  const lang = useActiveLanguage();
  const { data } = useTodayVocabulary(lang);
  const { data: myVocab, isLoading: isMyWordsLoading } = useMyVocabulary(lang);

  return (
    <div className="page-container page-section">
      <div className="flex items-start justify-between">
        <PageHeader
          icon={BookOpen}
          iconColor="bg-vocabulary/10 text-vocabulary"
          title="Vocabulary"
          description={
            data
              ? `${data.newWordsCount} new word${data.newWordsCount !== 1 ? "s" : ""}, ${data.reviewWordsCount} to review`
              : "Today's words to review and study."
          }
        />
        <StudySettingsDialog targetLanguageCode={lang} />
      </div>
      <div className="mt-6">
        <VocabularyList
          newItems={data?.newItems}
          reviewItems={data?.reviewItems}
          newWordsPerDay={data?.newWordsPerDay}
          myWords={myVocab?.items}
          myCounts={myVocab?.counts}
          isMyWordsLoading={isMyWordsLoading}
        />
      </div>
    </div>
  );
}
