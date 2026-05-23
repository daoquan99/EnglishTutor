export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  displayName: string;
}

export interface AuthTokenResponse {
  accessToken: string;
  expiresAtUtc: string;
}

export interface CurrentUser {
  userId: string;
  email: string;
  displayName: string;
  roles?: string[];
  permissions?: string[];
}
