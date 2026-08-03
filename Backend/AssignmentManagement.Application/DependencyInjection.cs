using AssignmentManagement.Application.Features.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace AssignmentManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}