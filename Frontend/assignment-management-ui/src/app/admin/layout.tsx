import { RequireRole } from "@/components/RequireRole";
import { Navbar } from "@/components/Navbar";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
    return (
        <RequireRole role="Admin">
            <Navbar />
            <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-6">{children}</main>
        </RequireRole>
    );
}