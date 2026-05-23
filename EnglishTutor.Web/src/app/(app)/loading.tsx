import { Skeleton } from "@/shared/components/ui/skeleton";

export default function AppLoading() {
  return (
    <div className="page-container page-section">
      <Skeleton className="h-8 w-48" />
      <Skeleton className="mt-4 h-4 w-64" />
      <div className="mt-6 grid gap-4">
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
    </div>
  );
}
