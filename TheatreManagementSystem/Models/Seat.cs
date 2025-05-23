using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheatreManagementSystem.Models
{
    [Table("seats")]
    public class Seat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column("theatre_id")]
        public long TheatreId { get; set; }

        [Required]
        public int ScreenNumber { get; set; }

        [Required]
        [StringLength(10)]
        public string RowName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Seat number must be positive")]
        public int SeatNumber { get; set; }

        [Required]
        public SeatType SeatType { get; set; }

        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Price multiplier must be positive or zero")]
        public double PriceMultiplier { get; set; }

        // Navigation properties
        [ForeignKey("TheatreId")]
        public virtual Theatre Theatre { get; set; } = null!;
    }

    public enum SeatType
    {
        STANDARD,
        PREMIUM,
        VIP,
        ACCESSIBLE
    }
}