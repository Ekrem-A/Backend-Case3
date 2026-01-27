using Identity.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// Get all users (Admin only)
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        
        var userList = new List<object>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                Roles = roles
            });
        }

        return Ok(userList);
    }

    
    /// Get user by ID (Admin only)
  
    [HttpGet("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            Roles = roles
        });
    }

    
    /// Assign role to user (Admin only)
  
    [HttpPost("users/{userId}/roles/{roleName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return BadRequest(new { Message = $"Role '{roleName}' does not exist." });
        }

        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            return BadRequest(new { Message = $"User already has role '{roleName}'." });
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (!result.Succeeded)
        {
            return BadRequest(new { Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        return Ok(new { Message = $"Role '{roleName}' assigned to user successfully." });
    }

    
    /// Remove role from user (Admin only)
  
    [HttpDelete("users/{userId}/roles/{roleName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            return BadRequest(new { Message = $"User does not have role '{roleName}'." });
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);

        if (!result.Succeeded)
        {
            return BadRequest(new { Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        return Ok(new { Message = $"Role '{roleName}' removed from user successfully." });
    }

    
    /// Get all roles (Admin only)
  
    [HttpGet("roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return Ok(roles.Select(r => new { r.Id, r.Name }));
    }

    
    /// Create new role (Admin only)
  
    [HttpPost("roles")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (await _roleManager.RoleExistsAsync(request.RoleName))
        {
            return BadRequest(new { Message = $"Role '{request.RoleName}' already exists." });
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(request.RoleName));

        if (!result.Succeeded)
        {
            return BadRequest(new { Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        return Created("", new { Message = $"Role '{request.RoleName}' created successfully." });
    }

    
    /// Delete role (Admin only)
  
    [HttpDelete("roles/{roleName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        
        if (role == null)
        {
            return NotFound(new { Message = $"Role '{roleName}' not found." });
        }

        if (roleName == "Admin")
        {
            return BadRequest(new { Message = "Cannot delete the Admin role." });
        }

        var result = await _roleManager.DeleteAsync(role);

        if (!result.Succeeded)
        {
            return BadRequest(new { Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        return Ok(new { Message = $"Role '{roleName}' deleted successfully." });
    }
}

public class UserStatusRequest
{
    public bool IsActive { get; set; }
}

public class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
}

