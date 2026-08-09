"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { useAuth } from "@/lib/auth-context";
import { ApiError } from "@/lib/api";
import { ErrorBanner } from "@/components/ErrorBanner";

const HOME_BY_ROLE: Record<string, string> = {
    Admin: "/admin/users",
    Teacher: "/teacher/assignments",
    Student: "/student/assignments",
};

export default function LoginPage() {
    const { login } = useAuth();
    const router = useRouter();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);
        setSubmitting(true);

        try {
            const user = await login(email, password);
            router.replace(HOME_BY_ROLE[user.role] ?? "/");
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Unable to log in. Please try again.");
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className="flex min-h-screen items-center justify-center px-4">
            <div className="w-full max-w-sm rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
                <h1 className="mb-1 text-xl font-semibold text-gray-900">Sign in</h1>
                <p className="mb-6 text-sm text-gray-500">Assignment & Submission Management System</p>

                <ErrorBanner message={error} />

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="mb-1 block text-sm font-medium text-gray-700">Email</label>
                        <input
                            type="email"
                            required
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                            placeholder="you@school.test"
                        />
                    </div>
                    <div>
                        <label className="mb-1 block text-sm font-medium text-gray-700">Password</label>
                        <input
                            type="password"
                            required
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                            placeholder="••••••••"
                        />
                    </div>
                    <button
                        type="submit"
                        disabled={submitting}
                        className="w-full rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                    >
                        {submitting ? "Signing in..." : "Sign in"}
                    </button>
                </form>

                <div className="mt-6 rounded-md bg-gray-50 p-3 text-xs text-gray-500">
                    <p className="mb-1 font-medium text-gray-600">Demo accounts</p>
                    <p>admin@school.test / Admin@123</p>
                    <p>teacher@school.test / Teacher@123</p>
                    <p>student@school.test / Student@123</p>
                </div>
            </div>
        </div>
    );
}