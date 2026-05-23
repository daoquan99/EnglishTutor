"use client";

import { Menu, PanelLeftClose, PanelLeft } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { ThemeToggle } from "@/shared/components/theme-toggle";
import { NotificationPopover } from "@/features/notifications/components/notification-popover";
import { UserMenu } from "@/features/auth/components/user-menu";
import { Separator } from "@/shared/components/ui/separator";

interface AppHeaderProps {
  onMenuClick: () => void;
  collapsed: boolean;
  onToggleCollapse: () => void;
}

export function AppHeader({ onMenuClick, collapsed, onToggleCollapse }: AppHeaderProps) {
  return (
    <header className="sticky top-0 z-30 flex h-14 items-center gap-2 border-b bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <Button variant="ghost" size="icon" className="md:hidden" onClick={onMenuClick}>
        <Menu className="size-5" />
        <span className="sr-only">Open menu</span>
      </Button>

      <Button
        variant="ghost"
        size="icon-sm"
        className="hidden md:flex"
        onClick={onToggleCollapse}
      >
        {collapsed ? <PanelLeft className="size-4" /> : <PanelLeftClose className="size-4" />}
      </Button>

      <div className="flex-1" />

      <div className="flex items-center gap-1">
        <NotificationPopover />
        <ThemeToggle />
        <Separator orientation="vertical" className="mx-1 h-6" />
        <UserMenu />
      </div>
    </header>
  );
}
