interface PageHeaderProps {
  icon?: React.ElementType;
  iconColor?: string;
  title: string;
  description?: string;
  action?: React.ReactNode;
}

export function PageHeader({ icon: Icon, iconColor, title, description, action }: PageHeaderProps) {
  return (
    <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex items-start gap-3">
        {Icon && (
          <div className={`mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-lg ${iconColor ?? "bg-primary/10"}`}>
            <Icon className={`size-4.5 ${iconColor ? "" : "text-primary"}`} />
          </div>
        )}
        <div>
          <h1 className="font-heading text-2xl font-bold tracking-tight">{title}</h1>
          {description && (
            <p className="mt-0.5 text-sm text-muted-foreground">{description}</p>
          )}
        </div>
      </div>
      {action && <div className="shrink-0">{action}</div>}
    </div>
  );
}
