using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.AS;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string UserName { get; set; } = null!;

    public string? Email { get; set; }

    [MaxLength(100)]
    public string PasswordHash { get; set; } = null!;

    [MaxLength(50)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(200)]
    public string? AvatarUrl { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool? IsActive { get; set; } = false;

    [MaxLength(50)]
    public string? Role { get; set; } = "User";
}