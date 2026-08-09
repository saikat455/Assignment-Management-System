"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { TeacherSubjectOption } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

function toLocalDatetimeInputValue(date: Date): string {
    const pad = (n: number) => n.toString().padStart(2, "0");
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export default function NewAssignmentPage() {
    const router = useRouter();
    const [subjects, setSubjects] = useState<TeacherSubjectOption[]>([]);
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [subjectId, setSubjectId] = useState("");
    const [deadline, setDeadline] = useState(() => {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 7);
        return toLocalDatetimeInputValue(tomorrow);
    });
    const [maxMarks, setMaxMarks] = useState(100);
    const [publishImmediately, setPublishImmediately] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        apiFetch<TeacherSubjectOption[]>("/api/teacher/subjects").then(setSubjects).catch(() => { });
    }, []);

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);

        if (!subjectId) {
            setError("Please select a subject.");
            return;
        }

        setSubmitting(true);
        try {
            await apiFetch("/api/teacher/assignments", {
                method: "POST",
                body: {
                    title,
                    description,
                    subjectId,
                    deadlineUtc: new Date(deadline).toISOString(),
                    maxMarks,
                    publishImmediately,
                },
            });
            router.push("/teacher/assignments");
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to create assignment.");
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className="max-w-md">
            <h1 className="mb-4 text-lg font-semibold text-gray-900">New assignment</h1>
            <ErrorBanner message={error} />

            {subjects.length === 0 && (
                <p className="mb-4 text-sm text-gray-500">
                    You aren&apos;t assigned to any subjects yet. Ask an administrator to assign you to a subject first.
                </p>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
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
                    <label className="mb-1 block text-sm font-medium text-gray-700">Subject</label>
                    <select
                        value={subjectId}
                        onChange={(e) => setSubjectId(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    >
                        <option value="">Select a subject</option>
                        {subjects.map((s) => (
                            <option key={s.subjectId} value={s.subjectId}>
                                {s.subjectName} ({s.className})
                            </option>
                        ))}
                    </select>
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
                <label className="flex items-center gap-2 text-sm text-gray-700">
                    <input
                        type="checkbox"
                        checked={publishImmediately}
                        onChange={(e) => setPublishImmediately(e.target.checked)}
                    />
                    Publish immediately (otherwise saved as a draft)
                </label>
                <div className="flex gap-2">
                    <button
                        type="submit"
                        disabled={submitting}
                        className="rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                    >
                        {submitting ? "Creating..." : "Create assignment"}
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