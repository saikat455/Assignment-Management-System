import { RequireRole } from "@/components/RequireRole";
import { Navbar } from "@/components/Navbar";

export default function TeacherLayout({ children }: { children: React.ReactNode }) {
    return (
        <RequireRole role="Teacher">
            <Navbar />
            <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-6">{children}</main>
        </RequireRole>
    );
}