"use client";

import { Menu } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { ThemeToggle } from "@/shared/components/theme-toggle";
import { UserMenu } from "@/features/auth/components/user-menu";
import { Separator } from "@/shared/components/ui/separator";

interface AdminHeaderProps {
  onMenuClick: () => void;
}

export function AdminHeader({ onMenuClick }: AdminHeaderProps) {
  return (
    <header className="sticky top-0 z-30 flex h-14 items-center gap-2 border-b bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <Button variant="ghost" size="icon" className="md:hidden" onClick={onMenuClick}>
        <Menu className="size-5" />
        <span className="sr-only">Open menu</span>
      </Button>

      <div className="hidden items-center gap-1.5 md:flex">
        <span className="rounded-md bg-destructive/10 px-2 py-0.5 text-xs font-medium text-destructive">
          Admin
        </span>
      </div>

      <div className="flex-1" />

      <div className="flex items-center gap-1">
        <ThemeToggle />
        <Separator orientation="vertical" className="mx-1 h-6" />
        <UserMenu />
      </div>
    </header>
  );
}
