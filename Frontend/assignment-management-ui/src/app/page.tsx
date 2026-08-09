"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { useAuth } from "@/lib/auth-context";

const HOME_BY_ROLE: Record<string, string> = {
  Admin: "/admin/users",
  Teacher: "/teacher/assignments",
  Student: "/student/assignments",
};

export default function HomePage() {
  const { user, loading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (loading) return;

    if (!user) {
      router.replace("/login");
      return;
    }

    router.replace(HOME_BY_ROLE[user.role] ?? "/login");
  }, [loading, user, router]);

  return (
    <div className="flex min-h-screen items-center justify-center text-sm text-gray-500">
      Loading...
    </div>
  );
}