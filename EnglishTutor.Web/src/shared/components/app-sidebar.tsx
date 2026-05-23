"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  BookOpen,
  BrainCircuit,
  ChevronLeft,
  GraduationCap,
  LayoutDashboard,
  MessageSquare,
  Mic,
  NotebookPen,
  Settings,
  Trophy,
  X,
} from "lucide-react";
import { cn } from "@/shared/lib/utils";
import { Button } from "@/shared/components/ui/button";

const navSections = [
  {
    label: "Overview",
    items: [
      { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
      { href: "/study-plan", label: "Study Plan", icon: NotebookPen },
    ],
  },
  {
    label: "Learn",
    items: [
      { href: "/vocabulary", label: "Vocabulary", icon: BookOpen },
      { href: "/exercises", label: "Exercises", icon: BrainCircuit },
      { href: "/speaking", label: "Speaking", icon: Mic },
    ],
  },
  {
    label: "Track",
    items: [
      { href: "/assessments", label: "Assessments", icon: GraduationCap },
      { href: "/mistakes", label: "Mistakes", icon: MessageSquare },
      { href: "/progress", label: "Progress", icon: Trophy },
    ],
  },
];

const bottomItems = [
  { href: "/settings", label: "Settings", icon: Settings },
];

interface AppSidebarProps {
  open: boolean;
  collapsed: boolean;
  onClose: () => void;
  onToggleCollapse: () => void;
}

export function AppSidebar({ open, collapsed, onClose, onToggleCollapse }: AppSidebarProps) {
  const pathname = usePathname();

  return (
    <>
      {open && (
        <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm md:hidden" onClick={onClose} />
      )}

      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-50 flex flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground transition-all duration-200 ease-in-out md:static md:translate-x-0",
          open ? "translate-x-0" : "-translate-x-full",
          collapsed ? "md:w-16" : "md:w-60",
          "w-64",
        )}
      >
        <div className={cn(
          "flex h-14 items-center border-b border-sidebar-border",
          collapsed ? "justify-center px-2" : "justify-between px-4",
        )}>
          {collapsed ? (
            <Link href="/dashboard" className="text-lg font-bold text-primary">
              ET
            </Link>
          ) : (
            <Link href="/dashboard" className="flex items-center gap-2" onClick={onClose}>
              <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-xs font-bold text-primary-foreground">
                ET
              </div>
              <span className="text-sm font-semibold">EnglishTutor</span>
            </Link>
          )}

          <Button variant="ghost" size="icon-xs" className="md:hidden" onClick={onClose}>
            <X className="size-4" />
          </Button>

          <Button
            variant="ghost"
            size="icon-xs"
            className="hidden md:flex"
            onClick={onToggleCollapse}
          >
            <ChevronLeft className={cn("size-4 transition-transform", collapsed && "rotate-180")} />
          </Button>
        </div>

        <nav className="flex-1 overflow-y-auto px-2 py-3">
          {navSections.map((section) => (
            <div key={section.label} className="mb-4">
              {!collapsed && (
                <p className="mb-1.5 px-3 text-[11px] font-semibold uppercase tracking-wider text-sidebar-foreground/40">
                  {section.label}
                </p>
              )}
              <ul className="grid gap-0.5">
                {section.items.map(({ href, label, icon: Icon }) => {
                  const active = pathname === href || pathname.startsWith(href + "/");
                  return (
                    <li key={href}>
                      <Link
                        href={href}
                        onClick={onClose}
                        title={collapsed ? label : undefined}
                        className={cn(
                          "group relative flex items-center rounded-lg transition-colors",
                          collapsed ? "justify-center px-2 py-2" : "gap-2.5 px-3 py-2",
                          active
                            ? "bg-sidebar-accent text-sidebar-accent-foreground font-medium"
                            : "text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-accent-foreground",
                        )}
                      >
                        {active && (
                          <span className="absolute left-0 top-1/2 h-5 w-0.5 -translate-y-1/2 rounded-full bg-primary" />
                        )}
                        <Icon className="size-4 shrink-0" />
                        {!collapsed && <span className="text-sm">{label}</span>}
                      </Link>
                    </li>
                  );
                })}
              </ul>
            </div>
          ))}
        </nav>

        <div className="border-t border-sidebar-border px-2 py-2">
          {bottomItems.map(({ href, label, icon: Icon }) => {
            const active = pathname === href;
            return (
              <Link
                key={href}
                href={href}
                onClick={onClose}
                title={collapsed ? label : undefined}
                className={cn(
                  "flex items-center rounded-lg transition-colors",
                  collapsed ? "justify-center px-2 py-2" : "gap-2.5 px-3 py-2",
                  active
                    ? "bg-sidebar-accent text-sidebar-accent-foreground font-medium"
                    : "text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-accent-foreground",
                )}
              >
                <Icon className="size-4 shrink-0" />
                {!collapsed && <span className="text-sm">{label}</span>}
              </Link>
            );
          })}
        </div>
      </aside>
    </>
  );
}
