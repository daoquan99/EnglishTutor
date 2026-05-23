import { HubConnectionBuilder, HubConnection, LogLevel } from "@microsoft/signalr";
import { getAccessToken } from "@/shared/api/auth-token-store";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

let connection: HubConnection | null = null;

export function getNotificationConnection(): HubConnection {
  if (!connection) {
    connection = new HubConnectionBuilder()
      .withUrl(`${BASE_URL}/hubs/notifications`, {
        accessTokenFactory: () => getAccessToken() ?? "",
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();
  }
  return connection;
}

export function disposeNotificationConnection(): void {
  if (connection) {
    connection.stop();
    connection = null;
  }
}
