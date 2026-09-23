namespace UserManagementAPI.Models;

using System.ComponentModel.DataAnnotations;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string? FirstName { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string? LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string? Email { get; set; }
}
