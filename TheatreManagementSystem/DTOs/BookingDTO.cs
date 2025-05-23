using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.DTOs
{
    public class BookingDTO
    {
        public long? Id { get; set; }
        public string? BookingNumber { get; set; }
        public long UserId { get; set; }
        public string? Username { get; set; }
        public string? UserEmail { get; set; }
        public long ScreeningId { get; set; }
        public string? MovieTitle { get; set; }
        public long? MovieId { get; set; }
        public long? TheatreId { get; set; }
        public string? MovieUrl { get; set; }
        public string? TheatreName { get; set; }
        public DateTime ScreeningTime { get; set; }
        public DateTime BookingTime { get; set; }
        public double TotalAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public ICollection<string> BookedSeats { get; set; } = new List<string>();
        public string? PaymentMethod { get; set; }
    }
}