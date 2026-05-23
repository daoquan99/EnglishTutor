import { httpClient } from "@/shared/api";
import type { AuthTokenResponse, CurrentUser, LoginRequest, RegisterRequest } from "../types/auth";

export const authApi = {
  login: (data: LoginRequest) =>
    httpClient.post<AuthTokenResponse>("/api/auth/login", data),

  register: (data: RegisterRequest) =>
    httpClient.post<AuthTokenResponse>("/api/auth/register", data),

  refreshToken: () => httpClient.post<AuthTokenResponse>("/api/auth/refresh-token"),

  logout: () => httpClient.post<void>("/api/auth/logout"),

  me: () => httpClient.get<CurrentUser>("/api/auth/me"),
};
