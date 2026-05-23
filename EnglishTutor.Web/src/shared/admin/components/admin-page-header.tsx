import type { ReactNode } from "react";
import { ChevronRight } from "lucide-react";
import Link from "next/link";

export type AdminBreadcrumbItem = {
  label: string;
  href?: string;
};

type AdminPageHeaderProps = {
  title: string;
  description?: string;
  breadcrumb?: AdminBreadcrumbItem[];
  primaryAction?: ReactNode;
};

export function AdminPageHeader({
  title,
  description,
  breadcrumb,
  primaryAction,
}: AdminPageHeaderProps) {
  return (
    <div className="mb-6 flex flex-col gap-3 border-b pb-4 sm:flex-row sm:items-end sm:justify-between">
      <div className="space-y-1">
        {breadcrumb && breadcrumb.length > 0 ? (
          <nav
            aria-label="Breadcrumb"
            className="flex items-center gap-1 text-xs text-muted-foreground"
          >
            {breadcrumb.map((item, index) => {
              const isLast = index === breadcrumb.length - 1;
              return (
                <span key={`${item.label}-${index}`} className="flex items-center gap-1">
                  {item.href && !isLast ? (
                    <Link href={item.href} className="hover:text-foreground">
                      {item.label}
                    </Link>
                  ) : (
                    <span className={isLast ? "text-foreground" : ""}>{item.label}</span>
                  )}
                  {!isLast ? <ChevronRight className="size-3" /> : null}
                </span>
              );
            })}
          </nav>
        ) : null}
        <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
        {description ? (
          <p className="text-sm text-muted-foreground">{description}</p>
        ) : null}
      </div>
      {primaryAction ? <div className="shrink-0">{primaryAction}</div> : null}
    </div>
  );
}
