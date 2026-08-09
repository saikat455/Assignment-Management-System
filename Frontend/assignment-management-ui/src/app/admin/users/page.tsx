"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { Role, UserResponse } from "@/lib/types";
import { ErrorBanner } from "@/components/ErrorBanner";

export default function AdminUsersPage() {
    const [users, setUsers] = useState<UserResponse[]>([]);
    const [roleFilter, setRoleFilter] = useState<Role | "">("");
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            const query = roleFilter ? `?role=${roleFilter}` : "";
            const data = await apiFetch<UserResponse[]>(`/api/admin/users${query}`);
            setUsers(data);
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to load users.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [roleFilter]);

    async function handleDeactivate(id: string) {
        if (!confirm("Deactivate this user? They will no longer be able to log in.")) return;

        try {
            await apiFetch(`/api/admin/users/${id}`, { method: "DELETE" });
            load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to deactivate user.");
        }
    }

    return (
        <div>
            <div className="mb-4 flex items-center justify-between">
                <h1 className="text-lg font-semibold text-gray-900">Users</h1>
                <Link
                    href="/admin/users/new"
                    className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-800"
                >
                    New user
                </Link>
            </div>

            <ErrorBanner message={error} />

            <div className="mb-4">
                <select
                    value={roleFilter}
                    onChange={(e) => setRoleFilter(e.target.value as Role | "")}
                    className="rounded-md border border-gray-300 px-3 py-1.5 text-sm"
                >
                    <option value="">All roles</option>
                    <option value="Admin">Admin</option>
                    <option value="Teacher">Teacher</option>
                    <option value="Student">Student</option>
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
                                <th className="px-4 py-2">Email</th>
                                <th className="px-4 py-2">Role</th>
                                <th className="px-4 py-2">Class</th>
                                <th className="px-4 py-2">Status</th>
                                <th className="px-4 py-2"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {users.map((user) => (
                                <tr key={user.id} className="border-b border-gray-100 last:border-0">
                                    <td className="px-4 py-2">{user.fullName}</td>
                                    <td className="px-4 py-2 text-gray-500">{user.email}</td>
                                    <td className="px-4 py-2">{user.role}</td>
                                    <td className="px-4 py-2 text-gray-500">{user.className ?? "-"}</td>
                                    <td className="px-4 py-2">
                                        <span className={user.isActive ? "text-green-600" : "text-red-500"}>
                                            {user.isActive ? "Active" : "Inactive"}
                                        </span>
                                    </td>
                                    <td className="px-4 py-2 text-right">
                                        <Link href={`/admin/users/${user.id}`} className="mr-3 text-blue-600 hover:underline">
                                            Edit
                                        </Link>
                                        {user.isActive && (
                                            <button onClick={() => handleDeactivate(user.id)} className="text-red-600 hover:underline">
                                                Deactivate
                                            </button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                            {users.length === 0 && (
                                <tr>
                                    <td colSpan={6} className="px-4 py-6 text-center text-gray-400">
                                        No users found.
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