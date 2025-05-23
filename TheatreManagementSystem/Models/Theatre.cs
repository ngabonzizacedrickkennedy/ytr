using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheatreManagementSystem.Models
{
    [Table("theatres")]
    public class Theatre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total screens must be a positive number")]
        public int? TotalScreens { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        // Navigation properties
        public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}