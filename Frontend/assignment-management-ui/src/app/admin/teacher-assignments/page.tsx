"use client";

import { useEffect, useState, type FormEvent } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { SubjectResponse, TeacherAssignmentResponse, UserResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

export default function AdminTeacherAssignmentsPage() {
    const [assignments, setAssignments] = useState<TeacherAssignmentResponse[]>([]);
    const [teachers, setTeachers] = useState<UserResponse[]>([]);
    const [subjects, setSubjects] = useState<SubjectResponse[]>([]);
    const [teacherId, setTeacherId] = useState("");
    const [subjectId, setSubjectId] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            const [assignmentList, teacherList, subjectList] = await Promise.all([
                apiFetch<TeacherAssignmentResponse[]>("/api/admin/teacher-assignments"),
                apiFetch<UserResponse[]>("/api/admin/users?role=Teacher"),
                apiFetch<SubjectResponse[]>("/api/admin/subjects"),
            ]);
            setAssignments(assignmentList);
            setTeachers(teacherList.filter((t) => t.isActive));
            setSubjects(subjectList);
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to load teacher assignments.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
    }, []);

    async function handleAssign(event: FormEvent) {
        event.preventDefault();
        setError(null);

        if (!teacherId || !subjectId) {
            setError("Please select both a teacher and a subject.");
            return;
        }

        setSubmitting(true);
        try {
            await apiFetch("/api/admin/teacher-assignments", { method: "POST", body: { teacherId, subjectId } });
            setTeacherId("");
            setSubjectId("");
            load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to assign teacher.");
        } finally {
            setSubmitting(false);
        }
    }

    async function handleUnassign(id: string) {
        if (!confirm("Remove this teacher assignment?")) return;

        try {
            await apiFetch(`/api/admin/teacher-assignments/${id}`, { method: "DELETE" });
            load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to unassign teacher.");
        }
    }

    return (
        <div>
            <h1 className="mb-4 text-lg font-semibold text-gray-900">Teacher Assignments</h1>

            <ErrorBanner message={error} />

            <form onSubmit={handleAssign} className="mb-6 flex flex-wrap items-end gap-3 rounded-md border border-gray-200 bg-white p-4">
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Teacher</label>
                    <select
                        value={teacherId}
                        onChange={(e) => setTeacherId(e.target.value)}
                        className="rounded-md border border-gray-300 px-3 py-2 text-sm"
                    >
                        <option value="">Select teacher</option>
                        {teachers.map((t) => (
                            <option key={t.id} value={t.id}>
                                {t.fullName}
                            </option>
                        ))}
                    </select>
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Subject</label>
                    <select
                        value={subjectId}
                        onChange={(e) => setSubjectId(e.target.value)}
                        className="rounded-md border border-gray-300 px-3 py-2 text-sm"
                    >
                        <option value="">Select subject</option>
                        {subjects.map((s) => (
                            <option key={s.id} value={s.id}>
                                {s.name} ({s.className})
                            </option>
                        ))}
                    </select>
                </div>
                <button
                    type="submit"
                    disabled={submitting}
                    className="rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                >
                    {submitting ? "Assigning..." : "Assign"}
                </button>
            </form>

            {loading ? (
                <p className="text-sm text-gray-500">Loading...</p>
            ) : (
                <div className="overflow-hidden rounded-md border border-gray-200 bg-white">
                    <table className="w-full text-left text-sm">
                        <thead className="border-b border-gray-200 bg-gray-50 text-gray-500">
                            <tr>
                                <th className="px-4 py-2">Teacher</th>
                                <th className="px-4 py-2">Subject</th>
                                <th className="px-4 py-2">Class</th>
                                <th className="px-4 py-2"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {assignments.map((a) => (
                                <tr key={a.id} className="border-b border-gray-100 last:border-0">
                                    <td className="px-4 py-2">{a.teacherName}</td>
                                    <td className="px-4 py-2">{a.subjectName}</td>
                                    <td className="px-4 py-2 text-gray-500">{a.className}</td>
                                    <td className="px-4 py-2 text-right">
                                        <button onClick={() => handleUnassign(a.id)} className="text-red-600 hover:underline">
                                            Unassign
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {assignments.length === 0 && (
                                <tr>
                                    <td colSpan={4} className="px-4 py-6 text-center text-gray-400">
                                        No teacher assignments yet.
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