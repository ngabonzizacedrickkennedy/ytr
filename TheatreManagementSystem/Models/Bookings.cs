using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheatreManagementSystem.Models
{
    [Table("bookings")]
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column("user_id")]
        public long UserId { get; set; }

        [Required]
        [Column("screening_id")]
        public long ScreeningId { get; set; }

        [Required]
        [StringLength(50)]
        public string BookingNumber { get; set; } = string.Empty;

        [Required]
        public DateTime BookingTime { get; set; }

        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Total amount must be positive or zero")]
        public double TotalAmount { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; }

        // Store booked seats as a comma-separated string or JSON
        // In Spring Boot, this was handled with @ElementCollection
        // In EF Core, we'll use a simple string approach for now
        public string BookedSeats { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ScreeningId")]
        public virtual Screening Screening { get; set; } = null!;

        // Helper property to work with booked seats as a collection
        [NotMapped]
        public ICollection<string> BookedSeatsCollection
        {
            get => string.IsNullOrEmpty(BookedSeats)
                ? new List<string>()
                : BookedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            set => BookedSeats = string.Join(",", value);
        }
    }

    public enum PaymentStatus
    {
        PENDING,
        COMPLETED,
        CANCELLED,
        REFUNDED
    }
}