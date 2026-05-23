let accessToken: string | null = null;
let expiresAt: Date | null = null;

export function setAccessToken(token: string, expiresAtUtc: string): void {
  accessToken = token;
  expiresAt = new Date(expiresAtUtc);
}

export function getAccessToken(): string | null {
  if (!accessToken || isTokenExpired()) {
    return null;
  }
  return accessToken;
}

export function clearAccessToken(): void {
  accessToken = null;
  expiresAt = null;
}

export function isTokenExpired(): boolean {
  if (!expiresAt) return true;
  const bufferMs = 30_000;
  return Date.now() >= expiresAt.getTime() - bufferMs;
}
