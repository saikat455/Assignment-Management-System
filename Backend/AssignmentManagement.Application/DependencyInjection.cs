using AssignmentManagement.Application.Features.Admin.Classes;
using AssignmentManagement.Application.Features.Admin.Subjects;
using AssignmentManagement.Application.Features.Admin.TeacherAssignments;
using AssignmentManagement.Application.Features.Admin.Users;
using AssignmentManagement.Application.Features.Auth;
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

        return services;
    }
}