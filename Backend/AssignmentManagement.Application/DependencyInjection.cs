using AssignmentManagement.Application.Features.Admin.Assignments;
using AssignmentManagement.Application.Features.Admin.Classes;
using AssignmentManagement.Application.Features.Admin.Subjects;
using AssignmentManagement.Application.Features.Admin.TeacherAssignments;
using AssignmentManagement.Application.Features.Admin.Users;
using AssignmentManagement.Application.Features.Auth;
using AssignmentManagement.Application.Features.Student.Assignments;
using AssignmentManagement.Application.Features.Student.Submissions;
using AssignmentManagement.Application.Features.Teacher.Assignments;
using AssignmentManagement.Application.Features.Teacher.Submissions;
using AssignmentManagement.Application.Features.Teacher.Subjects;
using Microsoft.Extensions.DependencyInjection;

namespace AssignmentManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IAdminAssignmentService, AdminAssignmentService>();
        services.AddScoped<IStudentAssignmentService, StudentAssignmentService>();
        services.AddScoped<IStudentSubmissionService, StudentSubmissionService>();
        services.AddScoped<ITeacherSubmissionService, TeacherSubmissionService>();
        services.AddScoped<ITeacherSubjectService, TeacherSubjectService>();

        return services;
    }
}