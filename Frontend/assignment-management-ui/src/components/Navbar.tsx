"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";

const LINKS: Record<string, { href: string; label: string }[]> = {
    Admin: [
        { href: "/admin/users", label: "Users" },
        { href: "/admin/classes", label: "Classes" },
        { href: "/admin/subjects", label: "Subjects" },
        { href: "/admin/teacher-assignments", label: "Teacher Assignments" },
        { href: "/admin/assignments", label: "All Assignments" },
    ],
    Teacher: [{ href: "/teacher/assignments", label: "My Assignments" }],
    Student: [
        { href: "/student/assignments", label: "Assignments" },
        { href: "/student/submissions", label: "My Submissions" },
    ],
};

export function Navbar() {
    const { user, logout } = useAuth();
    const router = useRouter();

    if (!user) return null;

    function handleLogout() {
        logout();
        router.replace("/login");
    }

    return (
        <nav className="border-b border-gray-200 bg-white">
            <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
                <div className="flex items-center gap-6">
                    <span className="font-semibold text-gray-900">Assignment Management</span>
                    <div className="flex gap-4">
                        {(LINKS[user.role] ?? []).map((link) => (
                            <Link key={link.href} href={link.href} className="text-sm text-gray-600 hover:text-gray-900">
                                {link.label}
                            </Link>
                        ))}
                    </div>
                </div>
                <div className="flex items-center gap-3">
                    <span className="text-sm text-gray-500">
                        {user.fullName} <span className="text-gray-400">({user.role})</span>
                    </span>
                    <button
                        onClick={handleLogout}
                        className="rounded-md border border-gray-300 px-3 py-1 text-sm text-gray-700 hover:bg-gray-50"
                    >
                        Log out
                    </button>
                </div>
            </div>
        </nav>
    );
}