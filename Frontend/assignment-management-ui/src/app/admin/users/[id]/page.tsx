"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { ClassResponse, Role, UserResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

export default function EditUserPage() {
    const { id } = useParams<{ id: string }>();
    const router = useRouter();

    const [classes, setClasses] = useState<ClassResponse[]>([]);
    const [fullName, setFullName] = useState("");
    const [email, setEmail] = useState("");
    const [role, setRole] = useState<Role>("Student");
    const [isActive, setIsActive] = useState(true);
    const [classId, setClassId] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        async function load() {
            try {
                const [user, classList] = await Promise.all([
                    apiFetch<UserResponse>(`/api/admin/users/${id}`),
                    apiFetch<ClassResponse[]>("/api/admin/classes"),
                ]);
                setFullName(user.fullName);
                setEmail(user.email);
                setRole(user.role);
                setIsActive(user.isActive);
                setClassId(user.classId ?? "");
                setClasses(classList);
            } catch (err) {
                setError(err instanceof ApiError ? err.message : "Failed to load user.");
            } finally {
                setLoading(false);
            }
        }
        load();
    }, [id]);

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);

        if (role === "Student" && !classId) {
            setError("Please select a class for the student.");
            return;
        }

        setSubmitting(true);
        try {
            await apiFetch(`/api/admin/users/${id}`, {
                method: "PUT",
                body: {
                    fullName,
                    email,
                    role,
                    isActive,
                    classId: role === "Student" ? classId : null,
                },
            });
            router.push("/admin/users");
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to update user.");
        } finally {
            setSubmitting(false);
        }
    }

    if (loading) return <p className="text-sm text-gray-500">Loading...</p>;

    return (
        <div className="max-w-md">
            <h1 className="mb-4 text-lg font-semibold text-gray-900">Edit user</h1>

            <ErrorBanner message={error} />

            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Full name</label>
                    <input
                        required
                        value={fullName}
                        onChange={(e) => setFullName(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Email</label>
                    <input
                        type="email"
                        required
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    />
                </div>
                <div>
                    <label className="mb-1 block text-sm font-medium text-gray-700">Role</label>
                    <select
                        value={role}
                        onChange={(e) => setRole(e.target.value as Role)}
                        className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                    >
                        <option value="Admin">Admin</option>
                        <option value="Teacher">Teacher</option>
                        <option value="Student">Student</option>
                    </select>
                </div>
                {role === "Student" && (
                    <div>
                        <label className="mb-1 block text-sm font-medium text-gray-700">Class</label>
                        <select
                            value={classId}
                            onChange={(e) => setClassId(e.target.value)}
                            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                        >
                            <option value="">Select a class</option>
                            {classes.map((c) => (
                                <option key={c.id} value={c.id}>
                                    {c.name}
                                    {c.section ? ` - ${c.section}` : ""}
                                </option>
                            ))}
                        </select>
                    </div>
                )}
                <label className="flex items-center gap-2 text-sm text-gray-700">
                    <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
                    Active
                </label>
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
                        onClick={() => router.push("/admin/users")}
                        className="rounded-md border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                    >
                        Cancel
                    </button>
                </div>
            </form>
        </div>
    );
}