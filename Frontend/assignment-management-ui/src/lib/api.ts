import type { ApiErrorPayload } from "./types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "https://localhost:5001";

export class ApiError extends Error {
    status: number;

    constructor(status: number, message: string) {
        super(message);
        this.status = status;
    }
}

function getToken(): string | null {
    if (typeof window === "undefined") return null;
    return window.localStorage.getItem("token");
}

/**
 * Thin fetch wrapper: attaches the JWT (if present), serializes JSON bodies,
 * and turns the backend's { status, message } error shape into an ApiError
 * so callers can show a real message instead of a generic failure.
 */
export async function apiFetch<T>(
    path: string,
    options: { method?: string; body?: unknown; skipAuth?: boolean } = {}
): Promise<T> {
    const { method = "GET", body, skipAuth = false } = options;

    const headers: Record<string, string> = {
        "Content-Type": "application/json",
    };

    if (!skipAuth) {
        const token = getToken();
        if (token) {
            headers["Authorization"] = `Bearer ${token}`;
        }
    }

    const response = await fetch(`${API_BASE_URL}${path}`, {
        method,
        headers,
        body: body !== undefined ? JSON.stringify(body) : undefined,
    });

    if (response.status === 204) {
        return undefined as T;
    }

    const text = await response.text();
    const data = text ? JSON.parse(text) : undefined;

    if (!response.ok) {
        const payload = data as ApiErrorPayload | undefined;
        throw new ApiError(response.status, payload?.message ?? "Something went wrong. Please try again.");
    }

    return data as T;
}