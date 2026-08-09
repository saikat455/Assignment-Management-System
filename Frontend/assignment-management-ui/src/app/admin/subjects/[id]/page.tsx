"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { SubjectResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

export default function EditSubjectPage() {
    const { id } = useParams<{ id: string }>();
    const router = useRouter();

    const [name, setName] = useState("");
    const [code, setCode] = useState("");
    const [className, setClassName] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        apiFetch<SubjectResponse>(`/api/admin/subjects/${id}`)
            .then((s) => {
                setName(s.name);
                setCode(s.code);
                setClassName(s.className);
            })
            .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load subject."))
            .finally(() => setLoading(false));
    }, [id]);

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);
        setSubmitting(true);
        try {
            await apiFetch(`/api/admin/subjects/${id}`, { method: "PUT", body: { name, code } });
            router.push("/admin/subjects");
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to update subject.");
        } finally {
            setSubmitting(false);
        }
    }

    if (loading) return <p className="text-sm text-gray-500">Loading...</p>;

    return (
        <div className="max-w-md">
            <h1 className="mb-4 text-lg font-semibold text-gray-900">Edit subject</h1>
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
                    <label className="mb-1 block text-sm font-medium text-gray-700">Code</label>
                    <input
                        required
                        value={code}
                        onChange={(e) => setCode(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Class</label>
                    <input
                        disabled
                        value={className}
                        className="w-full rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-500"
                    />
                    <p className="mt-1 text-xs text-gray-400">Class can&apos;t be changed after creation.</p>
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
                        onClick={() => router.push("/admin/subjects")}
                        className="rounded-md border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                        Cancel
                    </button>
                </div>
            </form>
        </div>
    );
}