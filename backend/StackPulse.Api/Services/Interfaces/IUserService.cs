using StackPulse.Api.DTOs.Users;

namespace StackPulse.Api.Services.Interfaces;

public interface IUserService
{
    Task<IReadOnlyCollection<UserListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDetailDto> CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDetailDto?> UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
