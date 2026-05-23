import { ShieldOff } from "lucide-react";

type AdminAccessDeniedProps = {
  title?: string;
  description?: string;
};

export function AdminAccessDenied({
  title = "Không có quyền truy cập",
  description = "Tài khoản của em không có quyền xem mục này. Liên hệ admin để được cấp permission.",
}: AdminAccessDeniedProps) {
  return (
    <div className="flex flex-1 items-center justify-center p-8">
      <div className="max-w-md text-center">
        <div className="mx-auto mb-4 flex size-14 items-center justify-center rounded-full bg-destructive/10 text-destructive">
          <ShieldOff className="size-7" />
        </div>
        <h1 className="text-lg font-semibold">{title}</h1>
        <p className="mt-1 text-sm text-muted-foreground">{description}</p>
      </div>
    </div>
  );
}
