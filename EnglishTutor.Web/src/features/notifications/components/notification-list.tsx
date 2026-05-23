"use client";

import { Loader2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import type { Notification } from "../types/notifications";
import { useMarkNotificationRead } from "../hooks/use-mark-notification-read";

function formatDate(utc: string) {
  return new Date(utc).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function NotificationList({
  notifications,
}: {
  notifications?: Notification[];
}) {
  const markRead = useMarkNotificationRead();

  if (!notifications) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-16 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!notifications.length) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No notifications.
      </p>
    );
  }

  return (
    <ul className="grid gap-2">
      {notifications.map((n) => (
        <li
          key={n.id}
          className={`rounded-lg border p-3 ${!n.isRead ? "bg-muted/30" : ""}`}
        >
          <div className="flex items-start justify-between gap-2">
            <div>
              <p className="text-sm font-medium">{n.title}</p>
              <p className="mt-0.5 text-xs text-muted-foreground">{n.body}</p>
              <p className="mt-1 text-xs text-muted-foreground">
                {formatDate(n.scheduledAtUtc)}
              </p>
            </div>
            {!n.isRead && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => markRead.mutate(n.id)}
                disabled={markRead.isPending}
              >
                {markRead.isPending && (
                  <Loader2 className="h-3 w-3 animate-spin" />
                )}
                Mark read
              </Button>
            )}
          </div>
        </li>
      ))}
    </ul>
  );
}
