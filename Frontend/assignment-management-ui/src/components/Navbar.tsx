"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { useAuth } from "@/lib/auth-context";

const LINKS: Record<string, { href: string; label: string }[]> = {
    Admin: [{ href: "/admin/users", label: "Users" }, { href: "/admin/classes", label: "Classes" }, { href: "/admin/subjects", label: "Subjects" }, { href: "/admin/teacher-assignments", label: "Teaching" }, { href: "/admin/assignments", label: "Assignments" }],
    Teacher: [{ href: "/teacher/assignments", label: "My assignments" }],
    Student: [{ href: "/student/assignments", label: "Assignments" }, { href: "/student/submissions", label: "My submissions" }],
};

export function Navbar() {
    const { user, logout } = useAuth();
    const router = useRouter();
    const pathname = usePathname();
    const [menuOpen, setMenuOpen] = useState(false);
    const [profileOpen, setProfileOpen] = useState(false);
    const profileMenuRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        function closeProfileMenu(event: MouseEvent) {
            if (profileMenuRef.current && !profileMenuRef.current.contains(event.target as Node)) setProfileOpen(false);
        }
        function closeOnEscape(event: KeyboardEvent) {
            if (event.key === "Escape") setProfileOpen(false);
        }
        document.addEventListener("mousedown", closeProfileMenu);
        document.addEventListener("keydown", closeOnEscape);
        return () => {
            document.removeEventListener("mousedown", closeProfileMenu);
            document.removeEventListener("keydown", closeOnEscape);
        };
    }, []);

    if (!user) return null;
    const links = LINKS[user.role] ?? [];
    const initials = user.fullName.split(" ").filter(Boolean).map((name) => name[0]).join("").slice(0, 2).toUpperCase();

    function handleLogout() { setProfileOpen(false); logout(); router.replace("/login"); }
    return <header className="app-navbar"><div className="app-navbar__inner">
        <Link href={links[0]?.href ?? "/"} className="brand" onClick={() => setMenuOpen(false)}><span className="brand__mark" aria-hidden="true">A</span><span><strong>Assignly</strong><small>Learning workspace</small></span></Link>
        <button className="nav-toggle" onClick={() => setMenuOpen(!menuOpen)} aria-label="Toggle navigation" aria-expanded={menuOpen}><span></span><span></span><span></span></button>
        <div className={`nav-content ${menuOpen ? "nav-content--open" : ""}`}><nav className="nav-links" aria-label="Primary navigation">
            {links.map((link) => { const active = pathname === link.href || (link.href !== "/" && pathname.startsWith(`${link.href}/`)); return <Link key={link.href} href={link.href} className={active ? "nav-link nav-link--active" : "nav-link"} onClick={() => setMenuOpen(false)}>{link.label}</Link>; })}
        </nav><div className="user-menu" ref={profileMenuRef}>
                <button type="button" className="profile-trigger" onClick={() => setProfileOpen((open) => !open)} aria-expanded={profileOpen} aria-haspopup="menu">
                    <span className="user-avatar" aria-hidden="true">{initials}</span><span className="user-details"><strong>{user.fullName}</strong></span><span className="profile-chevron" aria-hidden="true"></span>
                </button>
                {profileOpen && <div className="profile-dropdown" role="menu">
                    <div className="profile-dropdown__identity"><span className="user-avatar" aria-hidden="true">{initials}</span><span><strong>{user.fullName}</strong><small>{user.role}</small></span></div>
                    <button type="button" onClick={handleLogout} className="logout-button" role="menuitem">Sign out</button>
                </div>}
            </div></div>
    </div></header>;
}
