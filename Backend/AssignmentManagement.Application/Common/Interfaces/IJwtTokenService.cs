using AssignmentManagement.Application.Common.Models;
using AssignmentManagement.Domain.Entities;

namespace AssignmentManagement.Application.Common.Interfaces;

public interface IJwtTokenService
{
    TokenResult GenerateToken(User user);
}