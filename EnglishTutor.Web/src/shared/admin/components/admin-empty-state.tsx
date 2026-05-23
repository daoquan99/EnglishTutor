import type { ComponentType, ReactNode } from "react";
import { Inbox } from "lucide-react";

type AdminEmptyStateProps = {
  icon?: ComponentType<{ className?: string }>;
  title: string;
  description?: string;
  action?: ReactNode;
};

export function AdminEmptyState({
  icon: Icon = Inbox,
  title,
  description,
  action,
}: AdminEmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 rounded-md border border-dashed py-12 text-center">
      <Icon className="size-10 text-muted-foreground" />
      <div className="space-y-1">
        <p className="text-sm font-semibold">{title}</p>
        {description ? (
          <p className="text-xs text-muted-foreground">{description}</p>
        ) : null}
      </div>
      {action ? <div>{action}</div> : null}
    </div>
  );
}
