"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { AssignmentResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

function toLocalDatetimeInputValue(isoString: string): string {
    const date = new Date(isoString);
    const pad = (n: number) => n.toString().padStart(2, "0");
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export default function EditAssignmentPage() {
    const { id } = useParams<{ id: string }>();
    const router = useRouter();

    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [deadline, setDeadline] = useState("");
    const [maxMarks, setMaxMarks] = useState(100);
    const [subjectLabel, setSubjectLabel] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        apiFetch<AssignmentResponse>(`/api/teacher/assignments/${id}`)
            .then((a) => {
                setTitle(a.title);
                setDescription(a.description);
                setDeadline(toLocalDatetimeInputValue(a.deadlineUtc));
                setMaxMarks(a.maxMarks);
                setSubjectLabel(`${a.subjectName} (${a.className})`);
            })
            .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignment."))
            .finally(() => setLoading(false));
    }, [id]);

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);
        setSubmitting(true);
        try {
            await apiFetch(`/api/teacher/assignments/${id}`, {
                method: "PUT",
                body: {
                    title,
                    description,
                    deadlineUtc: new Date(deadline).toISOString(),
                    maxMarks,
                },
            });
            router.push("/teacher/assignments");
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to update assignment.");
        } finally {
            setSubmitting(false);
        }
    }

    if (loading) return <p className="text-sm text-gray-500">Loading...</p>;

    return (
        <div className="max-w-md">
            <h1 className="mb-4 text-lg font-semibold text-gray-900">Edit assignment</h1>
            <ErrorBanner message={error} />
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Subject</label>
                    <input
                        disabled
                        value={subjectLabel}
                        className="w-full rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-500"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Title</label>
                    <input
                        required
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Description</label>
                    <textarea
                        required
                        rows={4}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Deadline</label>
                    <input
                        type="datetime-local"
                        required
                        value={deadline}
                        onChange={(e) => setDeadline(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Max marks</label>
                    <input
                        type="number"
                        required
                        min={1}
                        value={maxMarks}
                        onChange={(e) => setMaxMarks(Number(e.target.value))}
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
                        onClick={() => router.push("/teacher/assignments")}
                        className="rounded-md border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                        Cancel
                    </button>
                </div>
            </form>
        </div>
    );
}