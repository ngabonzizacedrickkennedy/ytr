using System.ComponentModel.DataAnnotations;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.DTOs
{
    public class ScreeningDTO
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Movie is required")]
        public long MovieId { get; set; }

        public string? MovieTitle { get; set; }

        [Required(ErrorMessage = "Theatre is required")]
        public long TheatreId { get; set; }

        public string? TheatreName { get; set; }

        // Original startTime field (DateTime)
        public DateTime StartTime { get; set; }

        // New fields for form binding with improved names
        public string? StartDateString { get; set; }
        public string? StartTimeString { get; set; }

        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Screen number is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Screen number must be positive")]
        public int ScreenNumber { get; set; }

        [Required(ErrorMessage = "Format is required")]
        public ScreeningFormat Format { get; set; }

        [Required(ErrorMessage = "Base price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be positive")]
        public double BasePrice { get; set; }
    }
}