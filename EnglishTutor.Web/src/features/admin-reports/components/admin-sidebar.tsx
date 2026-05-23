"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Activity,
  AlertTriangle,
  BookCheck,
  Brain,
  FileText,
  Inbox,
  Key,
  LayoutDashboard,
  Route,
  Server,
  Shield,
  Users,
  X,
} from "lucide-react";
import { cn } from "@/shared/lib/utils";
import { Button } from "@/shared/components/ui/button";
import { useAdminPermission } from "@/shared/admin";
import {
  PermissionCodes,
  type PermissionCode,
} from "@/features/auth/lib/permission-codes";

type NavItem = {
  href: string;
  label: string;
  icon: typeof LayoutDashboard;
  /** User passes if they have ANY of these (admin.full_access always passes). */
  requireAny?: readonly PermissionCode[];
};

type NavSection = {
  label: string;
  items: NavItem[];
};

const NAV_SECTIONS: NavSection[] = [
  {
    label: "Overview",
    items: [
      // Dashboard accessible to any admin-area user (no requireAny → always visible).
      { href: "/admin", label: "Dashboard", icon: LayoutDashboard },
    ],
  },
  {
    label: "Users & Access",
    items: [
      {
        href: "/admin/users",
        label: "Users",
        icon: Users,
        requireAny: [
          PermissionCodes.AuthUsersRead,
          PermissionCodes.AuthUsersManage,
        ],
      },
      {
        href: "/admin/roles",
        label: "Roles",
        icon: Shield,
        requireAny: [
          PermissionCodes.AuthRolesRead,
          PermissionCodes.AuthRolesManage,
        ],
      },
      {
        href: "/admin/permissions",
        label: "Permissions",
        icon: Key,
        requireAny: [
          PermissionCodes.AuthPermissionsRead,
          PermissionCodes.AuthPermissionsManage,
        ],
      },
    ],
  },
  {
    label: "AI",
    items: [
      {
        href: "/admin/ai-usage",
        label: "AI Usage",
        icon: Brain,
        requireAny: [PermissionCodes.AiLogsRead, PermissionCodes.AiProvidersRead],
      },
      {
        href: "/admin/ai-providers",
        label: "Providers",
        icon: Server,
        requireAny: [
          PermissionCodes.AiProvidersRead,
          PermissionCodes.AiProvidersManage,
        ],
      },
      {
        href: "/admin/ai-routes",
        label: "Routes",
        icon: Route,
        requireAny: [
          PermissionCodes.AiRoutesRead,
          PermissionCodes.AiRoutesManage,
        ],
      },
    ],
  },
  {
    label: "Reports",
    items: [
      {
        href: "/admin/activity",
        label: "Activity",
        icon: Activity,
        requireAny: [PermissionCodes.ReportsRead],
      },
      {
        href: "/admin/mistakes",
        label: "Mistakes",
        icon: AlertTriangle,
        requireAny: [PermissionCodes.MistakesRead, PermissionCodes.ReportsRead],
      },
      {
        href: "/admin/assessments",
        label: "Assessments",
        icon: BookCheck,
        requireAny: [
          PermissionCodes.AssessmentsRead,
          PermissionCodes.ReportsRead,
        ],
      },
    ],
  },
  {
    label: "System",
    items: [
      {
        href: "/admin/audit-logs",
        label: "Audit Logs",
        icon: FileText,
        requireAny: [PermissionCodes.AuthSecurityEventsRead],
      },
      {
        href: "/admin/dead-letters",
        label: "Dead Letters",
        icon: Inbox,
        // Dead-letters is a wildcard-only area for now.
        requireAny: [PermissionCodes.FullAccess],
      },
    ],
  },
];

interface AdminSidebarProps {
  open: boolean;
  onClose: () => void;
}

export function AdminSidebar({ open, onClose }: AdminSidebarProps) {
  const pathname = usePathname();
  const { hasAny } = useAdminPermission();

  const visibleSections = NAV_SECTIONS
    .map((section) => ({
      ...section,
      items: section.items.filter((item) => hasAny(item.requireAny)),
    }))
    .filter((section) => section.items.length > 0);

  return (
    <>
      {open && (
        <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm md:hidden" onClick={onClose} />
      )}

      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground transition-transform duration-200 ease-in-out md:static md:w-60 md:translate-x-0",
          open ? "translate-x-0" : "-translate-x-full",
        )}
      >
        <div className="flex h-14 items-center justify-between border-b border-sidebar-border px-4">
          <Link href="/admin" className="flex items-center gap-2" onClick={onClose}>
            <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-destructive/80 text-xs font-bold text-white">
              A
            </div>
            <span className="text-sm font-semibold">Admin Panel</span>
          </Link>
          <Button variant="ghost" size="icon-xs" className="md:hidden" onClick={onClose}>
            <X className="size-4" />
          </Button>
        </div>

        <nav className="flex-1 overflow-y-auto px-2 py-3">
          {visibleSections.map((section) => (
            <div key={section.label} className="mb-4">
              <p className="mb-1.5 px-3 text-[11px] font-semibold uppercase tracking-wider text-sidebar-foreground/40">
                {section.label}
              </p>
              <ul className="grid gap-0.5">
                {section.items.map(({ href, label, icon: Icon }) => {
                  const active = pathname === href || (href !== "/admin" && pathname.startsWith(href + "/"));
                  return (
                    <li key={href}>
                      <Link
                        href={href}
                        onClick={onClose}
                        className={cn(
                          "group relative flex items-center gap-2.5 rounded-lg px-3 py-2 transition-colors",
                          active
                            ? "bg-sidebar-accent text-sidebar-accent-foreground font-medium"
                            : "text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-accent-foreground",
                        )}
                      >
                        {active && (
                          <span className="absolute left-0 top-1/2 h-5 w-0.5 -translate-y-1/2 rounded-full bg-destructive" />
                        )}
                        <Icon className="size-4 shrink-0" />
                        <span className="text-sm">{label}</span>
                      </Link>
                    </li>
                  );
                })}
              </ul>
            </div>
          ))}
        </nav>

        <div className="border-t border-sidebar-border px-2 py-2">
          <Link
            href="/dashboard"
            onClick={onClose}
            className="flex items-center gap-2.5 rounded-lg px-3 py-2 text-sidebar-foreground/70 transition-colors hover:bg-sidebar-accent/50 hover:text-sidebar-accent-foreground"
          >
            <LayoutDashboard className="size-4 shrink-0" />
            <span className="text-sm">Back to App</span>
          </Link>
        </div>
      </aside>
    </>
  );
}
