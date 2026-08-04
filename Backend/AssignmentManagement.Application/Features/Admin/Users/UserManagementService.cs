using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Features.Admin.Users.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Admin.Users;

public class UserManagementService : IUserManagementService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UserManagementService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<UserResponse>> GetAllAsync(Role? role, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Include(u => u.Class).AsQueryable();

        if (role is not null)
        {
            query = query.Where(u => u.Role == role);
        }

        var users = await query.OrderBy(u => u.FullName).ToListAsync(cancellationToken);

        return users.Select(u => ToResponse(u)).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await FindUserOrThrowAsync(id, cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindUserOrThrowAsync(id, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailTakenByAnotherUser = await _context.Users
            .AnyAsync(u => u.Id != id && u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailTakenByAnotherUser)
        {
            throw new ConflictException($"A user with email '{request.Email}' already exists.");
        }

        if (id == _currentUserService.UserId && !request.IsActive)
        {
            throw new BadRequestException("You cannot deactivate your own account.");
        }

        Guid? classId = null;

        if (request.Role == Role.Student)
        {
            if (request.ClassId is null)
            {
                throw new BadRequestException("ClassId is required for a Student.");
            }

            var classExists = await _context.Classes.AnyAsync(c => c.Id == request.ClassId, cancellationToken);
            if (!classExists)
            {
                throw new NotFoundException(nameof(SchoolClass), request.ClassId);
            }

            classId = request.ClassId;
        }

        user.FullName = request.FullName.Trim();
        user.Email = normalizedEmail;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.ClassId = classId;

        await _context.SaveChangesAsync(cancellationToken);

        string? className = null;
        if (classId is not null)
        {
            var schoolClass = await _context.Classes.FirstAsync(c => c.Id == classId, cancellationToken);
            className = FormatClassName(schoolClass);
        }

        return ToResponse(user, className);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == _currentUserService.UserId)
        {
            throw new BadRequestException("You cannot deactivate your own account.");
        }

        var user = await FindUserOrThrowAsync(id, cancellationToken);
        user.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> FindUserOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Include(u => u.Class)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user ?? throw new NotFoundException(nameof(User), id);
    }

    private static UserResponse ToResponse(User user, string? classNameOverride = null) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        ClassId = user.ClassId,
        ClassName = classNameOverride ?? (user.Class is null ? null : FormatClassName(user.Class)),
        CreatedAtUtc = user.CreatedAtUtc
    };

    private static string FormatClassName(SchoolClass schoolClass) =>
        string.IsNullOrWhiteSpace(schoolClass.Section) ? schoolClass.Name : $"{schoolClass.Name} - {schoolClass.Section}";
}