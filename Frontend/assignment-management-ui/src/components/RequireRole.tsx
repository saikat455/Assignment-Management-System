"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState, type ReactNode } from "react";
import { useAuth } from "@/lib/auth-context";
import type { Role } from "@/lib/types";

export function RequireRole({ role, children }: { role: Role; children: ReactNode }) {
    const { user, loading } = useAuth();
    const router = useRouter();
    const [mounted, setMounted] = useState(false);

    useEffect(() => {
        setMounted(true);
    }, []);

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

    // Render nothing (matching what the server sent) until we've mounted on
    // the client - avoids a hydration mismatch, since auth state only exists
    // in the browser (localStorage) and can't be known during SSR.
    if (!mounted || loading || !user || user.role !== role) {
        return (
            <div suppressHydrationWarning className="flex min-h-screen items-center justify-center text-sm text-gray-500">
                {mounted ? "Loading..." : null}
            </div>
        );
    }

    return <>{children}</>;
}