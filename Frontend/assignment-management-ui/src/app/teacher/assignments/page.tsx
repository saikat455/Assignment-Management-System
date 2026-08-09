"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { AssignmentResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";
import { StatusBadge } from "@/components/StatusBadge";

export default function TeacherAssignmentsPage() {
    const [assignments, setAssignments] = useState<AssignmentResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            setAssignments(await apiFetch<AssignmentResponse[]>("/api/teacher/assignments"));
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to load assignments.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
    }, []);

    async function handlePublishToggle(a: AssignmentResponse) {
        try {
            const action = a.status === "Draft" ? "publish" : "unpublish";
            await apiFetch(`/api/teacher/assignments/${a.id}/${action}`, { method: "PATCH" });
            load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to update status.");
        }
    }

    async function handleDelete(id: string) {
        if (!confirm("Delete this assignment? This is only possible if no student has submitted work for it.")) return;

        try {
            await apiFetch(`/api/teacher/assignments/${id}`, { method: "DELETE" });
            load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to delete assignment.");
        }
    }

    return (
        <div>
            <div className="mb-4 flex items-center justify-between">
                <h1 className="text-lg font-semibold text-gray-900">My Assignments</h1>
                <Link
                    href="/teacher/assignments/new"
                    className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-800"
                >
                    New assignment
                </Link>
            </div>

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
                                <th className="px-4 py-2">Deadline</th>
                                <th className="px-4 py-2">Max marks</th>
                                <th className="px-4 py-2">Status</th>
                                <th className="px-4 py-2"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {assignments.map((a) => (
                                <tr key={a.id} className="border-b border-gray-100 last:border-0">
                                    <td className="px-4 py-2">{a.title}</td>
                                    <td className="px-4 py-2 text-gray-500">
                                        {a.subjectName} ({a.className})
                                    </td>
                                    <td className="px-4 py-2 text-gray-500">{new Date(a.deadlineUtc).toLocaleString()}</td>
                                    <td className="px-4 py-2">{a.maxMarks}</td>
                                    <td className="px-4 py-2">
                                        <StatusBadge status={a.status} />
                                    </td>
                                    <td className="px-4 py-2 text-right whitespace-nowrap">
                                        <Link href={`/teacher/assignments/${a.id}/submissions`} className="mr-3 text-blue-600 hover:underline">
                                            Submissions
                                        </Link>
                                        <Link href={`/teacher/assignments/${a.id}`} className="mr-3 text-blue-600 hover:underline">
                                            Edit
                                        </Link>
                                        <button onClick={() => handlePublishToggle(a)} className="mr-3 text-gray-700 hover:underline">
                                            {a.status === "Draft" ? "Publish" : "Unpublish"}
                                        </button>
                                        <button onClick={() => handleDelete(a.id)} className="text-red-600 hover:underline">
                                            Delete
                                        </button>
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