import { Skeleton } from "@/shared/components/ui/skeleton";

export default function AdminLoading() {
  return (
    <div className="page-container page-section">
      <Skeleton className="h-8 w-48" />
      <Skeleton className="mt-4 h-60 w-full" />
    </div>
  );
}
