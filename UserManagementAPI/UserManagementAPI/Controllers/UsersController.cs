using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private static readonly List<User> Users = new()
    {
        new User { FirstName = "Alice", LastName = "Smith", Email = "alice@example.com" },
        new User { FirstName = "Bob", LastName = "Jones", Email = "bob@example.com" }
    };

    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;
    }

    // GET /api/users?page=1&pageSize=50
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize, 1, 100);

            var skip = (page - 1) * pageSize;
            var result = Users.Skip(skip).Take(pageSize).ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            return Problem("An unexpected error occurred while fetching users.", statusCode: 500);
        }
    }

    [HttpGet("{id:guid}")]
    public ActionResult<User> GetById(Guid id)
    {
        try
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by id {UserId}", id);
            return Problem("An unexpected error occurred while fetching the user.", statusCode: 500);
        }
    }

    [HttpPost]
    public ActionResult<User> Create([FromBody] User user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Prevent duplicate emails
            if (Users.Any(u => string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(user.Email), "A user with the same email already exists.");
                return ValidationProblem(ModelState);
            }

            user.Id = Guid.NewGuid();
            Users.Add(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return Problem("An unexpected error occurred while creating the user.", statusCode: 500);
        }
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] User updated)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var index = Users.FindIndex(u => u.Id == id);
            if (index == -1) return NotFound();

            // Prevent duplicate email assigned to a different user
            if (Users.Any(u => u.Id != id && string.Equals(u.Email, updated.Email, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(updated.Email), "Another user with the same email already exists.");
                return ValidationProblem(ModelState);
            }

            updated.Id = id;
            Users[index] = updated;
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return Problem("An unexpected error occurred while updating the user.", statusCode: 500);
        }
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        try
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();
            Users.Remove(user);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return Problem("An unexpected error occurred while deleting the user.", statusCode: 500);
        }
    }
}
