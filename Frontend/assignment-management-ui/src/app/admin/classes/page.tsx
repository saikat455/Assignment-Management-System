"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { ClassResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

export default function AdminClassesPage() {
    const [classes, setClasses] = useState<ClassResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            setClasses(await apiFetch<ClassResponse[]>("/api/admin/classes"));
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to load classes.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
    }, []);

    async function handleDelete(id: string) {
        if (!confirm("Delete this class? This is only possible if it has no students or subjects.")) return;

        try {
            await apiFetch(`/api/admin/classes/${id}`, { method: "DELETE" });
            load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to delete class.");
        }
    }

    return (
        <div>
            <div className="mb-4 flex items-center justify-between">
                <h1 className="text-lg font-semibold text-gray-900">Classes</h1>
                <Link
                    href="/admin/classes/new"
                    className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-800"
                >
                    New class
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
                                <th className="px-4 py-2">Name</th>
                                <th className="px-4 py-2">Section</th>
                                <th className="px-4 py-2">Students</th>
                                <th className="px-4 py-2">Subjects</th>
                                <th className="px-4 py-2"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {classes.map((c) => (
                                <tr key={c.id} className="border-b border-gray-100 last:border-0">
                                    <td className="px-4 py-2">{c.name}</td>
                                    <td className="px-4 py-2 text-gray-500">{c.section ?? "-"}</td>
                                    <td className="px-4 py-2">{c.studentCount}</td>
                                    <td className="px-4 py-2">{c.subjectCount}</td>
                                    <td className="px-4 py-2 text-right">
                                        <Link href={`/admin/classes/${c.id}`} className="mr-3 text-blue-600 hover:underline">
                                            Edit
                                        </Link>
                                        <button onClick={() => handleDelete(c.id)} className="text-red-600 hover:underline">
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {classes.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="px-4 py-6 text-center text-gray-400">
                                        No classes yet.
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