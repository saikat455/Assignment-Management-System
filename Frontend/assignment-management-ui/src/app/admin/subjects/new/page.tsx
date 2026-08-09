"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { ClassResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

export default function NewSubjectPage() {
    const router = useRouter();
    const [classes, setClasses] = useState<ClassResponse[]>([]);
    const [name, setName] = useState("");
    const [code, setCode] = useState("");
    const [classId, setClassId] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        apiFetch<ClassResponse[]>("/api/admin/classes").then(setClasses).catch(() => { });
    }, []);

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);

        if (!classId) {
            setError("Please select a class.");
            return;
        }

        setSubmitting(true);
        try {
            await apiFetch("/api/admin/subjects", { method: "POST", body: { name, code, classId } });
            router.push("/admin/subjects");
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to create subject.");
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className="max-w-md">
            <h1 className="mb-4 text-lg font-semibold text-gray-900">New subject</h1>
            <ErrorBanner message={error} />
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Name</label>
                    <input
                        required
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="Mathematics"
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Code</label>
                    <input
                        required
                        value={code}
                        onChange={(e) => setCode(e.target.value)}
                        placeholder="MATH101"
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Class</label>
                    <select
                        value={classId}
                        onChange={(e) => setClassId(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    >
                        <option value="">Select a class</option>
                        {classes.map((c) => (
                            <option key={c.id} value={c.id}>
                                {c.name}
                                {c.section ? ` - ${c.section}` : ""}
                            </option>
                        ))}
                    </select>
                </div>
                <div className="flex gap-2">
                    <button
                        type="submit"
                        disabled={submitting}
                        className="rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                    >
                        {submitting ? "Creating..." : "Create subject"}
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