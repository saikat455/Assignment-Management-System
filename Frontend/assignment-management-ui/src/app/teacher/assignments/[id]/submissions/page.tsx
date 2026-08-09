"use client";

import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { SubmissionStatus, TeacherSubmissionResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";
import { StatusBadge } from "@/components/StatusBadge";

function GradeRow({ submission, onGraded }: { submission: TeacherSubmissionResponse; onGraded: () => void }) {
    const [expanded, setExpanded] = useState(false);
    const [marks, setMarks] = useState(submission.marksObtained ?? 0);
    const [feedback, setFeedback] = useState(submission.feedback ?? "");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    async function handleGrade() {
        setError(null);
        setSubmitting(true);
        try {
            await apiFetch(`/api/teacher/submissions/${submission.id}/grade`, {
                method: "PATCH",
                body: { marksObtained: marks, feedback: feedback || null },
            });
            setExpanded(false);
            onGraded();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to save grade.");
        } finally {
            setSubmitting(false);
        }
    }

    async function handleStatusChange(status: SubmissionStatus) {
        setError(null);
        try {
            await apiFetch(`/api/teacher/submissions/${submission.id}/status`, { method: "PATCH", body: { status } });
            onGraded();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to change status.");
        }
    }

    return (
        <>
            <tr className="border-b border-gray-100">
                <td className="px-4 py-2">
                    {submission.studentName}
                    <div className="text-xs text-gray-400">{submission.studentEmail}</div>
                </td>
                <td className="px-4 py-2 text-gray-500">{new Date(submission.submittedAtUtc).toLocaleString()}</td>
                <td className="px-4 py-2">
                    <StatusBadge status={submission.status} />
                </td>
                <td className="px-4 py-2">
                    {submission.marksObtained !== null ? `${submission.marksObtained} / ${submission.maxMarks}` : "-"}
                </td>
                <td className="px-4 py-2 text-right whitespace-nowrap">
                    <button onClick={() => setExpanded((v) => !v)} className="mr-3 text-blue-600 hover:underline">
                        {expanded ? "Close" : "Review"}
                    </button>
                    {submission.status !== "Returned" && (
                        <button onClick={() => handleStatusChange("Returned")} className="text-orange-600 hover:underline">
                            Return for changes
                        </button>
                    )}
                </td>
            </tr>
            {expanded && (
                <tr className="border-b border-gray-100 bg-gray-50">
                    <td colSpan={5} className="px-4 py-4">
                        <ErrorBanner message={error} />
                        <p className="mb-3 whitespace-pre-wrap text-sm text-gray-700">{submission.answerText}</p>
                        <div className="flex flex-wrap items-end gap-3">
                            <div>
                                <label className="mb-1 block text-xs font-medium text-gray-600">Marks (out of {submission.maxMarks})</label>
                                <input
                                    type="number"
                                    min={0}
                                    max={submission.maxMarks}
                                    value={marks}
                                    onChange={(e) => setMarks(Number(e.target.value))}
                                    className="w-28 rounded-md border border-gray-300 px-3 py-1.5 text-sm"
                                />
                            </div>
                            <div className="flex-1">
                                <label className="mb-1 block text-xs font-medium text-gray-600">Feedback (optional)</label>
                                <input
                                    value={feedback}
                                    onChange={(e) => setFeedback(e.target.value)}
                                    className="w-full rounded-md border border-gray-300 px-3 py-1.5 text-sm"
                                />
                            </div>
                            <button
                                onClick={handleGrade}
                                disabled={submitting}
                                className="rounded-md bg-gray-900 px-4 py-1.5 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                            >
                                {submitting ? "Saving..." : "Save grade"}
                            </button>
                        </div>
                    </td>
                </tr>
            )}
        </>
    );
}

export default function AssignmentSubmissionsPage() {
    const { id } = useParams<{ id: string }>();
    const [submissions, setSubmissions] = useState<TeacherSubmissionResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            setSubmissions(await apiFetch<TeacherSubmissionResponse[]>(`/api/teacher/assignments/${id}/submissions`));
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to load submissions.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [id]);

    return (
        <div>
            <h1 className="mb-4 text-lg font-semibold text-gray-900">Submissions</h1>

            <ErrorBanner message={error} />

            {loading ? (
                <p className="text-sm text-gray-500">Loading...</p>
            ) : (
                <div className="overflow-hidden rounded-md border border-gray-200 bg-white">
                    <table className="w-full text-left text-sm">
                        <thead className="border-b border-gray-200 bg-gray-50 text-gray-500">
                            <tr>
                                <th className="px-4 py-2">Student</th>
                                <th className="px-4 py-2">Submitted</th>
                                <th className="px-4 py-2">Status</th>
                                <th className="px-4 py-2">Marks</th>
                                <th className="px-4 py-2"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {submissions.map((s) => (
                                <GradeRow key={s.id} submission={s} onGraded={load} />
                            ))}
                            {submissions.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="px-4 py-6 text-center text-gray-400">
                                        No submissions yet.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}