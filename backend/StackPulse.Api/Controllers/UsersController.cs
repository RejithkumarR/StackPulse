using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackPulse.Api.DTOs.Users;
using StackPulse.Api.Services.Interfaces;

namespace StackPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserListItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(new { success = true, message = "Users loaded successfully", data = users });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user is null) return NotFound(new { success = false, message = "User not found" });

        return Ok(new { success = true, message = "User loaded successfully", data = user });
    }

    [HttpPost]
    public async Task<ActionResult<UserDetailDto>> Create([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { success = true, message = "User created", data = user });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> Update(Guid id, [FromBody] UpdateUserRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateAsync(id, request, cancellationToken);
        if (user is null) return NotFound(new { success = false, message = "User not found" });

        return Ok(new { success = true, message = "User updated", data = user });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _userService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { success = false, message = "User not found" });

        return NoContent();
    }
}
