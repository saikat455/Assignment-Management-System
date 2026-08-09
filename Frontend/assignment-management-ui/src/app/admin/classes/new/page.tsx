"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import { ErrorBanner } from "@/components/ErrorBanner";

export default function NewClassPage() {
    const router = useRouter();
    const [name, setName] = useState("");
    const [section, setSection] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);
        setSubmitting(true);
        try {
            await apiFetch("/api/admin/classes", {
                method: "POST",
                body: { name, section: section || null },
            });
            router.push("/admin/classes");
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to create class.");
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className="max-w-md">
            <h1 className="mb-4 text-lg font-semibold text-gray-900">New class</h1>
            <ErrorBanner message={error} />
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Name</label>
                    <input
                        required
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="Class 10"
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Section (optional)</label>
                    <input
                        value={section}
                        onChange={(e) => setSection(e.target.value)}
                        placeholder="A"
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div className="flex gap-2">
                    <button
                        type="submit"
                        disabled={submitting}
                        className="rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                    >
                        {submitting ? "Creating..." : "Create class"}
                    </button>
                    <button
                        type="button"
                        onClick={() => router.push("/admin/classes")}
                        className="rounded-md border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                        Cancel
                    </button>
                </div>
            </form>
        </div>
    );
}