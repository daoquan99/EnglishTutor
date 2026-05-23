"use client";

import { use } from "react";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useLanguageSettings } from "@/features/users/hooks/use-language-settings";
import { useStudyCard } from "@/features/vocabulary/hooks/use-study-card";
import { StudyCardDetail } from "@/features/vocabulary/components/study-card-detail";

export default function VocabularyDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const lang = useActiveLanguage();
  const { data: langSettings } = useLanguageSettings();
  const { data: studyCard } = useStudyCard(
    id,
    lang,
    langSettings?.nativeLanguageCode ?? null,
  );

  return (
    <div className="page-container page-section">
      <Button variant="ghost" size="sm" render={<Link href="/vocabulary" />}>
        <ArrowLeft />
        Back to vocabulary
      </Button>

      <div className="mt-4">
        <StudyCardDetail data={studyCard} targetLanguageCode={lang} />
      </div>
    </div>
  );
}
