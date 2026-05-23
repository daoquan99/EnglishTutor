"use client";

import { use } from "react";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { useLessonDetail } from "@/features/learning-content/hooks/use-lesson-detail";
import { LessonDetailView } from "@/features/learning-content/components/lesson-detail-view";

export default function LessonDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: lesson } = useLessonDetail(id);

  return (
    <div className="page-container page-section">
      <Button variant="ghost" size="sm" render={<Link href="/lessons" />}>
        <ArrowLeft />
        Back to lessons
      </Button>
      <div className="mt-4">
        <LessonDetailView lesson={lesson} />
      </div>
    </div>
  );
}
