"use client";

import { BookOpen } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useLessons } from "@/features/learning-content/hooks/use-lessons";
import { LessonList } from "@/features/learning-content/components/lesson-list";

export default function LessonsPage() {
  const lang = useActiveLanguage();
  const { data } = useLessons({ targetLanguageCode: lang ?? undefined });

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={BookOpen}
        iconColor="bg-study/10 text-study"
        title="Lessons"
        description="Browse and study structured lessons."
      />
      <div className="mt-6">
        <LessonList lessons={data} />
      </div>
    </div>
  );
}
