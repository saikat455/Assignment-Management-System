const COLORS: Record<string, string> = {
    Draft: "bg-gray-100 text-gray-700",
    Published: "bg-green-100 text-green-700",
    Submitted: "bg-blue-100 text-blue-700",
    Late: "bg-amber-100 text-amber-700",
    Graded: "bg-emerald-100 text-emerald-700",
    Returned: "bg-orange-100 text-orange-700",
};

export function StatusBadge({ status }: { status: string }) {
    const colorClass = COLORS[status] ?? "bg-gray-100 text-gray-700";

    return (
        <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${colorClass}`}>
            {status}
        </span>
    );
}