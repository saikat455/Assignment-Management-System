"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { SubmissionResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";
import { StatusBadge } from "@/components/StatusBadge";

export default function StudentSubmissionsPage() {
    const [submissions, setSubmissions] = useState<SubmissionResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        apiFetch<SubmissionResponse[]>("/api/student/submissions")
            .then(setSubmissions)
            .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load submissions."))
            .finally(() => setLoading(false));
    }, []);

    return (
        <div>
            <h1 className="mb-4 text-lg font-semibold text-gray-900">My Submissions</h1>

            <ErrorBanner message={error} />

            {loading ? (
                <p className="text-sm text-gray-500">Loading...</p>
            ) : (
                <div className="overflow-hidden rounded-md border border-gray-200 bg-white">
                    <table className="w-full text-left text-sm">
                        <thead className="border-b border-gray-200 bg-gray-50 text-gray-500">
                            <tr>
                                <th className="px-4 py-2">Assignment</th>
                                <th className="px-4 py-2">Submitted</th>
                                <th className="px-4 py-2">Status</th>
                                <th className="px-4 py-2">Marks</th>
                                <th className="px-4 py-2"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {submissions.map((s) => (
                                <tr key={s.id} className="border-b border-gray-100 last:border-0">
                                    <td className="px-4 py-2">{s.assignmentTitle}</td>
                                    <td className="px-4 py-2 text-gray-500">{new Date(s.submittedAtUtc).toLocaleString()}</td>
                                    <td className="px-4 py-2">
                                        <StatusBadge status={s.status} />
                                    </td>
                                    <td className="px-4 py-2">{s.marksObtained !== null ? `${s.marksObtained} / ${s.maxMarks}` : "-"}</td>
                                    <td className="px-4 py-2 text-right">
                                        <Link href={`/student/assignments/${s.assignmentId}`} className="text-blue-600 hover:underline">
                                            View
                                        </Link>
                                    </td>
                                </tr>
                            ))}
                            {submissions.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="px-4 py-6 text-center text-gray-400">
                                        You haven&apos;t submitted anything yet.
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