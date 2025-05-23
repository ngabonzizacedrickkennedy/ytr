using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheatreManagementSystem.Models
{
    [Table("screenings")]
    public class Screening
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column("movie_id")]
        public long MovieId { get; set; }

        [Required]
        [Column("theatre_id")]
        public long TheatreId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int ScreenNumber { get; set; }

        [Required]
        public ScreeningFormat Format { get; set; }

        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Base price must be positive or zero")]
        public double BasePrice { get; set; }

        // Navigation properties
        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; } = null!;

        [ForeignKey("TheatreId")]
        public virtual Theatre Theatre { get; set; } = null!;

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public enum ScreeningFormat
    {
        STANDARD,
        IMAX,
        DOLBY_ATMOS,
        THREE_D,
        FOUR_D
    }
}