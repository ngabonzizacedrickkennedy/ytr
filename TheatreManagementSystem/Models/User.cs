using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TheatreManagementSystem.Models
{
    [Table("users")]
    public class User : IdentityUser<long>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }

        [Required]
        [StringLength(20)]
        public override string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [EmailAddress]
        public override string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public override string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public override string? PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.ROLE_USER;

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        // Override properties we don't need from IdentityUser
        public override string? NormalizedUserName { get; set; }
        public override string? NormalizedEmail { get; set; }
        public override bool EmailConfirmed { get; set; }
        public override string? SecurityStamp { get; set; }
        public override string? ConcurrencyStamp { get; set; }
        public override bool PhoneNumberConfirmed { get; set; }
        public override bool TwoFactorEnabled { get; set; }
        public override DateTimeOffset? LockoutEnd { get; set; }
        public override bool LockoutEnabled { get; set; }
        public override int AccessFailedCount { get; set; }
    }

    public enum UserRole
    {
        ROLE_USER,
        ROLE_MANAGER,
        ROLE_ADMIN
    }
}