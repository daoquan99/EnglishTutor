"use client";

import { useRealtimeNotifications } from "../hooks/use-realtime-notifications";

export function NotificationProvider() {
  useRealtimeNotifications();
  return null;
}
