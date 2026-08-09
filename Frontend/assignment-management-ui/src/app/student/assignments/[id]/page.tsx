"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { StudentAssignmentResponse, SubmissionResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";
import { StatusBadge } from "@/components/StatusBadge";

export default function StudentAssignmentDetailPage() {
    const { id } = useParams<{ id: string }>();
    const router = useRouter();

    const [assignment, setAssignment] = useState<StudentAssignmentResponse | null>(null);
    const [submission, setSubmission] = useState<SubmissionResponse | null>(null);
    const [answerText, setAnswerText] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            const a = await apiFetch<StudentAssignmentResponse>(`/api/student/assignments/${id}`);
            setAssignment(a);

            if (a.hasSubmitted) {
                const s = await apiFetch<SubmissionResponse>(`/api/student/submissions/${id}`);
                setSubmission(s);
                setAnswerText(s.answerText);
            }
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to load assignment.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [id]);

    const canEdit = !submission || submission.status === "Submitted" || submission.status === "Returned";

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);
        setSubmitting(true);
        try {
            if (submission) {
                await apiFetch(`/api/student/submissions/${id}`, { method: "PUT", body: { answerText } });
            } else {
                await apiFetch(`/api/student/submissions/${id}`, { method: "POST", body: { answerText } });
            }
            await load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to save your submission.");
        } finally {
            setSubmitting(false);
        }
    }

    if (loading) return <p className="text-sm text-gray-500">Loading...</p>;
    if (!assignment) return <ErrorBanner message={error ?? "Assignment not found."} />;

    return (
        <div className="max-w-2xl">
            <button onClick={() => router.push("/student/assignments")} className="mb-4 text-sm text-blue-600 hover:underline">
                &larr; Back to assignments
            </button>

            <div className="mb-6 rounded-md border border-gray-200 bg-white p-4">
                <h1 className="text-lg font-semibold text-gray-900">{assignment.title}</h1>
                <p className="mt-1 text-sm text-gray-500">
                    {assignment.subjectName} &middot; Taught by {assignment.teacherName}
                </p>
                <p className="mt-3 whitespace-pre-wrap text-sm text-gray-700">{assignment.description}</p>
                <div className="mt-4 flex flex-wrap gap-6 text-sm text-gray-600">
                    <span>Deadline: {new Date(assignment.deadlineUtc).toLocaleString()}</span>
                    <span>Max marks: {assignment.maxMarks}</span>
                </div>
            </div>

            <ErrorBanner message={error} />

            {submission && (
                <div className="mb-4 rounded-md border border-gray-200 bg-white p-4">
                    <div className="mb-2 flex items-center gap-2">
                        <span className="text-sm font-medium text-gray-700">Status:</span>
                        <StatusBadge status={submission.status} />
                    </div>
                    {submission.marksObtained !== null && (
                        <p className="text-sm text-gray-700">
                            Marks: <span className="font-medium">{submission.marksObtained}</span> / {submission.maxMarks}
                        </p>
                    )}
                    {submission.feedback && (
                        <p className="mt-2 text-sm text-gray-700">
                            <span className="font-medium">Feedback:</span> {submission.feedback}
                        </p>
                    )}
                </div>
            )}

            {canEdit ? (
                <form onSubmit={handleSubmit} className="rounded-md border border-gray-200 bg-white p-4">
                    <label className="mb-1 block text-sm font-medium text-gray-700">Your answer</label>
                    <textarea
                        required
                        rows={8}
                        value={answerText}
                        onChange={(e) => setAnswerText(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                    <button
                        type="submit"
                        disabled={submitting}
                        className="mt-3 rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                    >
                        {submitting ? "Saving..." : submission ? "Update submission" : "Submit"}
                    </button>
                </form>
            ) : (
                <div className="rounded-md border border-gray-200 bg-gray-50 p-4 text-sm text-gray-500">
                    This submission has been graded and can no longer be edited.
                </div>
            )}
        </div>
    );
}