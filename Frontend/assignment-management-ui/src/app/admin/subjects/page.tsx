"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { ClassResponse, SubjectResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

export default function AdminSubjectsPage() {
    const [subjects, setSubjects] = useState<SubjectResponse[]>([]);
    const [classes, setClasses] = useState<ClassResponse[]>([]);
    const [classFilter, setClassFilter] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        apiFetch<ClassResponse[]>("/api/admin/classes").then(setClasses).catch(() => { });
    }, []);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            const query = classFilter ? `?classId=${classFilter}` : "";
            setSubjects(await apiFetch<SubjectResponse[]>(`/api/admin/subjects${query}`));
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to load subjects.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [classFilter]);

    async function handleDelete(id: string) {
        if (!confirm("Delete this subject? This is only possible if no teacher is assigned to it.")) return;

        try {
            await apiFetch(`/api/admin/subjects/${id}`, { method: "DELETE" });
            load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to delete subject.");
        }
    }

    return (
        <div>
            <div className="mb-4 flex items-center justify-between">
                <h1 className="text-lg font-semibold text-gray-900">Subjects</h1>
                <Link
                    href="/admin/subjects/new"
                    className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-800"
                >
                    New subject
                </Link>
            </div>

            <ErrorBanner message={error} />

            <div className="mb-4">
                <select
                    value={classFilter}
                    onChange={(e) => setClassFilter(e.target.value)}
                    className="rounded-md border border-gray-300 px-3 py-1.5 text-sm"
                >
                    <option value="">All classes</option>
                    {classes.map((c) => (
                        <option key={c.id} value={c.id}>
                            {c.name}
                            {c.section ? ` - ${c.section}` : ""}
                        </option>
                    ))}
                </select>
            </div>

            {loading ? (
                <p className="text-sm text-gray-500">Loading...</p>
            ) : (
                <div className="overflow-hidden rounded-md border border-gray-200 bg-white">
                    <table className="w-full text-left text-sm">
                        <thead className="border-b border-gray-200 bg-gray-50 text-gray-500">
                            <tr>
                                <th className="px-4 py-2">Name</th>
                                <th className="px-4 py-2">Code</th>
                                <th className="px-4 py-2">Class</th>
                                <th className="px-4 py-2">Teachers</th>
                                <th className="px-4 py-2"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {subjects.map((s) => (
                                <tr key={s.id} className="border-b border-gray-100 last:border-0">
                                    <td className="px-4 py-2">{s.name}</td>
                                    <td className="px-4 py-2 text-gray-500">{s.code}</td>
                                    <td className="px-4 py-2 text-gray-500">{s.className}</td>
                                    <td className="px-4 py-2">{s.teacherCount}</td>
                                    <td className="px-4 py-2 text-right">
                                        <Link href={`/admin/subjects/${s.id}`} className="mr-3 text-blue-600 hover:underline">
                                            Edit
                                        </Link>
                                        <button onClick={() => handleDelete(s.id)} className="text-red-600 hover:underline">
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {subjects.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="px-4 py-6 text-center text-gray-400">
                                        No subjects yet.
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