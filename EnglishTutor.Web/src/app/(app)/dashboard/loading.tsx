import { Skeleton } from "@/shared/components/ui/skeleton";

export default function DashboardLoading() {
  return (
    <div className="page-container page-section">
      <Skeleton className="h-7 w-32" />
      <div className="mt-6 grid gap-4">
        <Skeleton className="h-40 w-full rounded-xl" />
        <div className="grid gap-4 md:grid-cols-2">
          <Skeleton className="h-32 w-full rounded-xl" />
          <Skeleton className="h-32 w-full rounded-xl" />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <Skeleton className="h-48 w-full rounded-xl" />
          <Skeleton className="h-48 w-full rounded-xl" />
        </div>
      </div>
    </div>
  );
}
