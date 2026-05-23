import { ApiError } from "./api-error";
import {
  getAccessToken,
  setAccessToken,
  clearAccessToken,
} from "./auth-token-store";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

type RequestOptions = Omit<RequestInit, "method" | "body"> & {
  params?: Record<string, string | number | boolean | undefined>;
};

type ApiEnvelope<T> = {
  isSuccess: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
    details?: Record<string, string[]>;
  };
};

let refreshPromise: Promise<boolean> | null = null;

function buildUrl(
  path: string,
  params?: Record<string, string | number | boolean | undefined>,
): string {
  const url = new URL(path, BASE_URL);
  if (params) {
    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined) {
        url.searchParams.set(key, String(value));
      }
    }
  }
  return url.toString();
}

function buildHeaders(custom?: HeadersInit): Headers {
  const headers = new Headers(custom);
  if (!headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }
  const token = getAccessToken();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }
  return headers;
}

function defaultErrorMessage(status: number): string {
  if (status === 400) return "The request was invalid. Please check your input.";
  if (status === 403) return "You do not have permission to perform this action.";
  if (status === 404) return "The requested resource was not found.";
  if (status === 409) return "This action conflicts with the current state.";
  if (status === 422) return "The request could not be processed.";
  if (status >= 500) return "A server error occurred. Please try again later.";
  return "An unexpected error occurred.";
}

async function parseResponse<T>(response: Response): Promise<T> {
  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get("content-type");
  if (!contentType?.includes("application/json")) {
    if (!response.ok) {
      throw new ApiError(
        response.status,
        undefined,
        defaultErrorMessage(response.status),
      );
    }
    return undefined as T;
  }

  const body = (await response.json()) as ApiEnvelope<T>;

  if (body.isSuccess === false || !response.ok) {
    throw new ApiError(
      response.status,
      body.error?.code,
      body.error?.message ?? defaultErrorMessage(response.status),
      body.error?.details,
    );
  }

  return body.data as T;
}

async function tryRefreshToken(): Promise<boolean> {
  try {
    const response = await fetch(buildUrl("/api/auth/refresh-token"), {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
    });

    if (!response.ok) return false;

    const body = (await response.json()) as ApiEnvelope<{
      accessToken: string;
      expiresAtUtc: string;
    }>;

    if (body.isSuccess && body.data) {
      setAccessToken(body.data.accessToken, body.data.expiresAtUtc);
      return true;
    }
    return false;
  } catch {
    return false;
  }
}

async function refreshOnce(): Promise<boolean> {
  if (!refreshPromise) {
    refreshPromise = tryRefreshToken().finally(() => {
      refreshPromise = null;
    });
  }
  return refreshPromise;
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown,
  options?: RequestOptions,
): Promise<T> {
  const { params, ...fetchOptions } = options ?? {};
  const url = buildUrl(path, params);
  const headers = buildHeaders(fetchOptions.headers);

  const response = await fetch(url, {
    ...fetchOptions,
    method,
    headers,
    credentials: "include",
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (response.status === 401) {
    const refreshed = await refreshOnce();
    if (refreshed) {
      const retryHeaders = buildHeaders(fetchOptions.headers);
      const retryResponse = await fetch(url, {
        ...fetchOptions,
        method,
        headers: retryHeaders,
        credentials: "include",
        body: body !== undefined ? JSON.stringify(body) : undefined,
      });
      return parseResponse<T>(retryResponse);
    }
    clearAccessToken();
    if (typeof window !== "undefined") {
      window.location.href = "/login";
    }
    throw new ApiError(401, "unauthorized", "Session expired");
  }

  return parseResponse<T>(response);
}

export const httpClient = {
  get<T>(path: string, options?: RequestOptions): Promise<T> {
    return request<T>("GET", path, undefined, options);
  },

  post<T>(path: string, body?: unknown, options?: RequestOptions): Promise<T> {
    return request<T>("POST", path, body, options);
  },

  put<T>(path: string, body?: unknown, options?: RequestOptions): Promise<T> {
    return request<T>("PUT", path, body, options);
  },

  patch<T>(
    path: string,
    body?: unknown,
    options?: RequestOptions,
  ): Promise<T> {
    return request<T>("PATCH", path, body, options);
  },

  delete<T>(path: string, options?: RequestOptions): Promise<T> {
    return request<T>("DELETE", path, undefined, options);
  },

  async postFormData<T>(
    path: string,
    formData: FormData,
    options?: RequestOptions,
  ): Promise<T> {
    const { params, ...fetchOptions } = options ?? {};
    const url = buildUrl(path, params);
    const headers = new Headers(fetchOptions.headers);
    const token = getAccessToken();
    if (token) {
      headers.set("Authorization", `Bearer ${token}`);
    }

    const response = await fetch(url, {
      ...fetchOptions,
      method: "POST",
      headers,
      credentials: "include",
      body: formData,
    });

    if (response.status === 401) {
      const refreshed = await refreshOnce();
      if (refreshed) {
        const retryHeaders = new Headers(fetchOptions.headers);
        const retryToken = getAccessToken();
        if (retryToken) {
          retryHeaders.set("Authorization", `Bearer ${retryToken}`);
        }
        const retryResponse = await fetch(url, {
          ...fetchOptions,
          method: "POST",
          headers: retryHeaders,
          credentials: "include",
          body: formData,
        });
        return parseResponse<T>(retryResponse);
      }
      clearAccessToken();
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
      throw new ApiError(401, "unauthorized", "Session expired");
    }

    return parseResponse<T>(response);
  },
};
