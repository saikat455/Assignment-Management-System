"use client";

import { useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { useAuth } from "@/lib/auth-context";
import type { Role } from "@/lib/types";

export function RequireRole({ role, children }: { role: Role; children: ReactNode }) {
    const { user, loading } = useAuth();
    const router = useRouter();

    useEffect(() => {
        if (loading) return;

        if (!user) {
            router.replace("/login");
            return;
        }

        if (user.role !== role) {
            router.replace("/");
        }
    }, [loading, user, role, router]);

    if (loading || !user || user.role !== role) {
        return (
            <div className="flex min-h-screen items-center justify-center text-sm text-gray-500">
                Loading...
            </div>
        );
    }

    return <>{children}</>;
}