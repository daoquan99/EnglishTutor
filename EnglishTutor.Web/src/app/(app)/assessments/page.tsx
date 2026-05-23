"use client";

import { GraduationCap } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { useActiveLanguage } from "@/shared/hooks/use-active-language";
import { useAvailableAssessments } from "@/features/assessments/hooks/use-available-assessments";
import { AvailableAssessmentsList } from "@/features/assessments/components/available-assessments-list";

export default function AssessmentsPage() {
  const lang = useActiveLanguage();
  const { data } = useAvailableAssessments(lang ?? undefined);

  return (
    <div className="page-container page-section">
      <PageHeader
        icon={GraduationCap}
        iconColor="bg-info/10 text-info"
        title="Assessments"
        description="Take assessments to measure your level and unlock new content."
      />
      <div className="mt-6">
        <AvailableAssessmentsList
          assessments={data}
          targetLanguageCode={lang ?? undefined}
        />
      </div>
    </div>
  );
}
