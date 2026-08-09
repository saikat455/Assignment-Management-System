"use client";

import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { apiFetch } from "./api";
import type { AuthResponse, AuthUser } from "./types";

interface AuthContextValue {
    user: AuthUser | null;
    token: string | null;
    loading: boolean;
    login: (email: string, password: string) => Promise<AuthUser>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const STORAGE_TOKEN_KEY = "token";
const STORAGE_USER_KEY = "authUser";

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<AuthUser | null>(null);
    const [token, setToken] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // Restore session from localStorage on first load. A JWT in
        // localStorage (rather than an httpOnly cookie) is a reasonable
        // trade-off for a project of this scope, but is worth calling out as a
        // known limitation for a production system.
        const storedToken = window.localStorage.getItem(STORAGE_TOKEN_KEY);
        const storedUser = window.localStorage.getItem(STORAGE_USER_KEY);

        if (storedToken && storedUser) {
            setToken(storedToken);
            setUser(JSON.parse(storedUser) as AuthUser);
        }

        setLoading(false);
    }, []);

    async function login(email: string, password: string): Promise<AuthUser> {
        const response = await apiFetch<AuthResponse>("/api/auth/login", {
            method: "POST",
            body: { email, password },
            skipAuth: true,
        });

        const authUser: AuthUser = {
            userId: response.userId,
            fullName: response.fullName,
            email: response.email,
            role: response.role,
        };

        window.localStorage.setItem(STORAGE_TOKEN_KEY, response.token);
        window.localStorage.setItem(STORAGE_USER_KEY, JSON.stringify(authUser));

        setToken(response.token);
        setUser(authUser);

        return authUser;
    }

    function logout() {
        window.localStorage.removeItem(STORAGE_TOKEN_KEY);
        window.localStorage.removeItem(STORAGE_USER_KEY);
        setToken(null);
        setUser(null);
    }

    return (
        <AuthContext.Provider value={{ user, token, loading, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth(): AuthContextValue {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
}