using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UserService.Contracts;
using UserService.Data;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(UserDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .ToListAsync(cancellationToken);

        return Ok(users.Select(UserResponse.From).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        return user is null ? NotFound() : Ok(UserResponse.From(user));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(
        UserRequest request,
        CancellationToken cancellationToken)
    {
        var usernameExists = await dbContext.Users
            .AnyAsync(user => user.Username == request.Username, cancellationToken);
        var emailExists = await dbContext.Users
            .AnyAsync(user => user.Email == request.Email, cancellationToken);

        if (usernameExists || emailExists)
        {
            return Conflict(new { message = "Username or email is already in use." });
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone,
            IsActive = request.IsActive
        };

        dbContext.Users.Add(user);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUsernameOrEmailConflict(exception))
        {
            return Conflict(new { message = "Username or email is already in use." });
        }

        var response = UserResponse.From(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Update(
        int id,
        UserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var duplicateExists = await dbContext.Users.AnyAsync(
            existing => existing.Id != id
                && (existing.Username == request.Username || existing.Email == request.Email),
            cancellationToken);

        if (duplicateExists)
        {
            return Conflict(new { message = "Username or email is already in use." });
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.IsActive = request.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUsernameOrEmailConflict(exception))
        {
            return Conflict(new { message = "Username or email is already in use." });
        }

        return Ok(UserResponse.From(user));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static bool IsUsernameOrEmailConflict(DbUpdateException exception) =>
        exception.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && postgresException.ConstraintName is "IX_dotnet_users_Username"
            or "IX_dotnet_users_Email";
}
