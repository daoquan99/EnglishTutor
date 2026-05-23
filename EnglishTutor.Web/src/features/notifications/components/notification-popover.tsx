"use client";

import { useRouter } from "next/navigation";
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
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useNotifications } from "../hooks/use-notifications";
import { useMarkNotificationRead } from "../hooks/use-mark-notification-read";
import { useMarkAllRead } from "../hooks/use-mark-all-read";
import { NotificationIcon } from "../lib/notification-icons";
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

function NotificationItem({
  notification,
  onNavigate,
}: {
  notification: Notification;
  onNavigate: (url: string) => void;
}) {
  const markRead = useMarkNotificationRead();

  const handleClick = () => {
    if (!notification.isRead) {
      markRead.mutate(notification.id);
    }
    if (notification.targetUrl) {
      onNavigate(notification.targetUrl);
    }
  };

  return (
    <button
      type="button"
      onClick={handleClick}
      className={`flex w-full gap-3 rounded-lg px-3 py-2.5 text-left transition-colors duration-150 hover:bg-muted/50 ${!notification.isRead ? "bg-primary/5" : ""} ${notification.targetUrl ? "cursor-pointer" : "cursor-default"}`}
    >
      <div className="mt-0.5">
        <NotificationIcon type={notification.type} />
      </div>
      <div className="min-w-0 flex-1">
        <p className="text-sm font-medium leading-snug">{notification.title}</p>
        <p className="mt-0.5 line-clamp-2 text-xs text-muted-foreground">{notification.body}</p>
        <p className="mt-1 text-xs text-muted-foreground/60">{formatTime(notification.scheduledAtUtc)}</p>
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
    </button>
  );
}

function NotificationSkeleton() {
  return (
    <div className="space-y-2 p-3">
      {Array.from({ length: 4 }, (_, i) => (
        <div key={i} className="flex gap-3">
          <Skeleton className="size-8 rounded-full" />
          <div className="flex-1 space-y-1.5">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-3 w-full" />
            <Skeleton className="h-3 w-16" />
          </div>
        </div>
      ))}
    </div>
  );
}

export function NotificationPopover() {
  const router = useRouter();
  const { data: all, isPending, isError, refetch } = useNotifications();
  const { data: unread } = useNotifications({ isRead: false });
  const markAllRead = useMarkAllRead();

  const unreadCount = unread?.length ?? 0;
  const notifications = all?.slice(0, 20) ?? [];

  const handleNavigate = (url: string) => {
    router.push(url);
  };

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

      <DropdownMenuContent align="end" sideOffset={8} className="w-[380px] p-0">
        <DropdownMenuGroup>
          <DropdownMenuLabel className="flex items-center justify-between px-4 py-3">
            <span className="text-sm font-semibold text-foreground">Notifications</span>
            <div className="flex items-center gap-2">
              {unreadCount > 0 && (
                <>
                  <span className="rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">
                    {unreadCount} new
                  </span>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-7 gap-1.5 px-2 text-xs text-muted-foreground"
                    onClick={() => markAllRead.mutate()}
                    disabled={markAllRead.isPending}
                  >
                    <CheckCheck className="size-3.5" />
                    Mark all read
                  </Button>
                </>
              )}
            </div>
          </DropdownMenuLabel>
        </DropdownMenuGroup>

        <DropdownMenuSeparator className="m-0" />

        <div className="max-h-[480px] overflow-y-auto p-1">
          {isPending ? (
            <NotificationSkeleton />
          ) : isError ? (
            <div className="flex flex-col items-center gap-2 py-10 text-center">
              <p className="text-sm text-muted-foreground">Could not load notifications.</p>
              <Button variant="outline" size="sm" onClick={() => refetch()}>
                Try again
              </Button>
            </div>
          ) : notifications.length === 0 ? (
            <div className="flex flex-col items-center gap-2 py-10 text-center">
              <CheckCheck className="size-8 text-muted-foreground/30" />
              <p className="text-sm text-muted-foreground">All caught up!</p>
            </div>
          ) : (
            <div className="grid gap-0.5">
              {notifications.map((n) => (
                <NotificationItem
                  key={n.id}
                  notification={n}
                  onNavigate={handleNavigate}
                />
              ))}
            </div>
          )}
        </div>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
