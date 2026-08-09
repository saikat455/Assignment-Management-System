"use client";

import { useEffect, useState } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { AssignmentResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";
import { StatusBadge } from "@/components/StatusBadge";

export default function AdminAssignmentsPage() {
    const [assignments, setAssignments] = useState<AssignmentResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        apiFetch<AssignmentResponse[]>("/api/admin/assignments")
            .then(setAssignments)
            .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignments."))
            .finally(() => setLoading(false));
    }, []);

    return (
        <div>
            <h1 className="mb-4 text-lg font-semibold text-gray-900">All Assignments</h1>

            <ErrorBanner message={error} />

            {loading ? (
                <p className="text-sm text-gray-500">Loading...</p>
            ) : (
                <div className="overflow-hidden rounded-md border border-gray-200 bg-white">
                    <table className="w-full text-left text-sm">
                        <thead className="border-b border-gray-200 bg-gray-50 text-gray-500">
                            <tr>
                                <th className="px-4 py-2">Title</th>
                                <th className="px-4 py-2">Subject</th>
                                <th className="px-4 py-2">Class</th>
                                <th className="px-4 py-2">Teacher</th>
                                <th className="px-4 py-2">Deadline</th>
                                <th className="px-4 py-2">Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {assignments.map((a) => (
                                <tr key={a.id} className="border-b border-gray-100 last:border-0">
                                    <td className="px-4 py-2">{a.title}</td>
                                    <td className="px-4 py-2 text-gray-500">{a.subjectName}</td>
                                    <td className="px-4 py-2 text-gray-500">{a.className}</td>
                                    <td className="px-4 py-2 text-gray-500">{a.teacherName}</td>
                                    <td className="px-4 py-2 text-gray-500">{new Date(a.deadlineUtc).toLocaleString()}</td>
                                    <td className="px-4 py-2">
                                        <StatusBadge status={a.status} />
                                    </td>
                                </tr>
                            ))}
                            {assignments.length === 0 && (
                                <tr>
                                    <td colSpan={6} className="px-4 py-6 text-center text-gray-400">
                                        No assignments yet.
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