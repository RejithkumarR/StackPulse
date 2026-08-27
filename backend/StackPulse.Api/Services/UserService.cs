using Microsoft.EntityFrameworkCore;
using StackPulse.Api.Data;
using StackPulse.Api.DTOs.Users;
using StackPulse.Api.Models;
using StackPulse.Api.Services.Interfaces;

namespace StackPulse.Api.Services;

public class UserService : IUserService
{
    private readonly StackPulseDbContext _dbContext;

    public UserService(StackPulseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<UserListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.users
            .AsNoTracking()
            .Include(u => u.Role)
            .OrderBy(u => u.Username)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                Role = u.Role.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Id == id)
            .Select(u => new UserDetailDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                Role = u.Role.Name,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDetailDto> CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.users.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        if (await _dbContext.users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken)
            ?? new Role { Id = Guid.NewGuid(), Name = "User", Description = "Standard user" };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UserDetailDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Role = role.Name,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<UserDetailDto?> UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            if (await _dbContext.users.AnyAsync(u => u.Email == request.Email && u.Id != id, cancellationToken))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            user.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.IsActive.ToString()))
        {
            user.IsActive = request.IsActive;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        _dbContext.users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
