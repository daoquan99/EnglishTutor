"use client";

import { Bell, Check, CheckCheck } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu";
import { useNotifications } from "../hooks/use-notifications";
import { useMarkNotificationRead } from "../hooks/use-mark-notification-read";
import type { Notification } from "../types/notifications";

function formatTime(utc: string) {
  const date = new Date(utc);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMin = Math.floor(diffMs / 60_000);
  if (diffMin < 1) return "Just now";
  if (diffMin < 60) return `${diffMin}m ago`;
  const diffHr = Math.floor(diffMin / 60);
  if (diffHr < 24) return `${diffHr}h ago`;
  const diffDay = Math.floor(diffHr / 24);
  if (diffDay < 7) return `${diffDay}d ago`;
  return date.toLocaleDateString(undefined, { month: "short", day: "numeric" });
}

function NotificationItem({ notification }: { notification: Notification }) {
  const markRead = useMarkNotificationRead();

  return (
    <div className={`flex gap-3 rounded-lg px-3 py-2.5 ${!notification.isRead ? "bg-primary/5" : ""}`}>
      <div className="mt-0.5 flex size-2 shrink-0">
        {!notification.isRead && (
          <span className="size-2 rounded-full bg-primary" />
        )}
      </div>
      <div className="min-w-0 flex-1">
        <p className="text-sm font-medium leading-snug">{notification.title}</p>
        <p className="mt-0.5 truncate text-xs text-muted-foreground">{notification.body}</p>
        <p className="mt-1 text-xs text-muted-foreground/70">{formatTime(notification.scheduledAtUtc)}</p>
      </div>
      {!notification.isRead && (
        <Button
          variant="ghost"
          size="icon-xs"
          className="mt-0.5 shrink-0"
          onClick={(e) => {
            e.stopPropagation();
            markRead.mutate(notification.id);
          }}
          disabled={markRead.isPending}
        >
          <Check className="size-3.5" />
          <span className="sr-only">Mark as read</span>
        </Button>
      )}
    </div>
  );
}

export function NotificationBell() {
  const { data: all } = useNotifications();
  const { data: unread } = useNotifications({ isRead: false });
  const unreadCount = unread?.length ?? 0;

  const notifications = all?.slice(0, 20) ?? [];

  return (
    <DropdownMenu>
      <DropdownMenuTrigger className="relative inline-flex size-9 cursor-pointer items-center justify-center rounded-full outline-none transition-colors hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring">
        <Bell className="size-[1.125rem]" />
        {unreadCount > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-medium text-destructive-foreground">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        )}
        <span className="sr-only">Notifications</span>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" sideOffset={8} className="w-80 p-0 sm:w-96">
        <DropdownMenuGroup>
          <DropdownMenuLabel className="flex items-center justify-between px-4 py-3">
            <span className="text-sm font-semibold text-foreground">Notifications</span>
            {unreadCount > 0 && (
              <span className="rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">
                {unreadCount} new
              </span>
            )}
          </DropdownMenuLabel>
        </DropdownMenuGroup>

        <DropdownMenuSeparator className="m-0" />

        <div className="max-h-80 overflow-y-auto p-1">
          {notifications.length === 0 ? (
            <div className="flex flex-col items-center gap-2 py-10 text-center">
              <CheckCheck className="size-8 text-muted-foreground/40" />
              <p className="text-sm text-muted-foreground">All caught up!</p>
            </div>
          ) : (
            <div className="grid gap-0.5">
              {notifications.map((n) => (
                <NotificationItem key={n.id} notification={n} />
              ))}
            </div>
          )}
        </div>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
