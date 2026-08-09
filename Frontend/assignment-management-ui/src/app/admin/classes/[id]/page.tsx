"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { ClassResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

export default function EditClassPage() {
    const { id } = useParams<{ id: string }>();
    const router = useRouter();

    const [name, setName] = useState("");
    const [section, setSection] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        apiFetch<ClassResponse>(`/api/admin/classes/${id}`)
            .then((c) => {
                setName(c.name);
                setSection(c.section ?? "");
            })
            .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load class."))
            .finally(() => setLoading(false));
    }, [id]);

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);
        setSubmitting(true);
        try {
            await apiFetch(`/api/admin/classes/${id}`, {
                method: "PUT",
                body: { name, section: section || null },
            });
            router.push("/admin/classes");
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to update class.");
        } finally {
            setSubmitting(false);
        }
    }

    if (loading) return <p className="text-sm text-gray-500">Loading...</p>;

    return (
        <div className="max-w-md">
            <h1 className="mb-4 text-lg font-semibold text-gray-900">Edit class</h1>
            <ErrorBanner message={error} />
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Name</label>
                    <input
                        required
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Section (optional)</label>
                    <input
                        value={section}
                        onChange={(e) => setSection(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div className="flex gap-2">
                    <button
                        type="submit"
                        disabled={submitting}
                        className="rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                    >
                        {submitting ? "Saving..." : "Save changes"}
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