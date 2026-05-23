"use client";

import { useEffect, useRef } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { getAccessToken } from "@/shared/api/auth-token-store";
import { notificationKeys } from "../api/query-keys";
import { getNotificationConnection, disposeNotificationConnection } from "../lib/signalr-connection";
import { showNotificationToast, type NotificationPushPayload } from "../lib/toast-helpers";

export function useRealtimeNotifications() {
  const queryClient = useQueryClient();
  const startedRef = useRef(false);

  useEffect(() => {
    const token = getAccessToken();
    if (!token) return;

    const connection = getNotificationConnection();

    const handleReceive = (raw: string) => {
      try {
        const payload = JSON.parse(raw) as NotificationPushPayload;
        queryClient.invalidateQueries({ queryKey: notificationKeys.all });
        showNotificationToast(payload);
      } catch {
        queryClient.invalidateQueries({ queryKey: notificationKeys.all });
      }
    };

    const handleReconnected = () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    };

    connection.on("ReceiveNotification", handleReceive);
    connection.onreconnected(handleReconnected);

    if (!startedRef.current && connection.state === HubConnectionState.Disconnected) {
      startedRef.current = true;
      connection.start().catch(() => {
        startedRef.current = false;
      });
    }

    return () => {
      connection.off("ReceiveNotification", handleReceive);
      disposeNotificationConnection();
      startedRef.current = false;
    };
  }, [queryClient]);
}
