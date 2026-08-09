export type Role = "Admin" | "Teacher" | "Student";

export type AssignmentStatus = "Draft" | "Published";

export type SubmissionStatus = "Submitted" | "Late" | "Graded" | "Returned";

export interface AuthUser {
    userId: string;
    fullName: string;
    email: string;
    role: Role;
}

export interface AuthResponse {
    token: string;
    expiresAtUtc: string;
    userId: string;
    fullName: string;
    email: string;
    role: Role;
}

export interface UserResponse {
    id: string;
    fullName: string;
    email: string;
    role: Role;
    isActive: boolean;
    classId: string | null;
    className: string | null;
    createdAtUtc: string;
}

export interface ClassResponse {
    id: string;
    name: string;
    section: string | null;
    studentCount: number;
    subjectCount: number;
    createdAtUtc: string;
}

export interface SubjectResponse {
    id: string;
    name: string;
    code: string;
    classId: string;
    className: string;
    teacherCount: number;
    createdAtUtc: string;
}

export interface TeacherAssignmentResponse {
    id: string;
    teacherId: string;
    teacherName: string;
    subjectId: string;
    subjectName: string;
    classId: string;
    className: string;
    createdAtUtc: string;
}

export interface AssignmentResponse {
    id: string;
    title: string;
    description: string;
    deadlineUtc: string;
    maxMarks: number;
    status: AssignmentStatus;
    subjectId: string;
    subjectName: string;
    classId: string;
    className: string;
    teacherId: string;
    teacherName: string;
    createdAtUtc: string;
    updatedAtUtc: string | null;
}

export interface StudentAssignmentResponse {
    id: string;
    title: string;
    description: string;
    deadlineUtc: string;
    isOverdue: boolean;
    maxMarks: number;
    subjectId: string;
    subjectName: string;
    teacherName: string;
    hasSubmitted: boolean;
    submissionStatus: SubmissionStatus | null;
    createdAtUtc: string;
}

export interface SubmissionResponse {
    id: string;
    assignmentId: string;
    assignmentTitle: string;
    answerText: string;
    status: SubmissionStatus;
    submittedAtUtc: string;
    updatedAtUtc: string | null;
    marksObtained: number | null;
    maxMarks: number;
    feedback: string | null;
}

export interface TeacherSubmissionResponse {
    id: string;
    assignmentId: string;
    assignmentTitle: string;
    maxMarks: number;
    studentId: string;
    studentName: string;
    studentEmail: string;
    answerText: string;
    status: SubmissionStatus;
    submittedAtUtc: string;
    updatedAtUtc: string | null;
    marksObtained: number | null;
    feedback: string | null;
}

export interface TeacherSubjectOption {
    subjectId: string;
    subjectName: string;
    classId: string;
    className: string;
}

export interface ApiErrorPayload {
    status: number;
    message: string;
}