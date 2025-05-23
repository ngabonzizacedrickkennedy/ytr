using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheatreManagementSystem.Models
{
    [Table("movies")]
    public class Movie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be positive")]
        public int DurationMinutes { get; set; }

        [Required]
        public MovieGenre Genre { get; set; }

        [StringLength(255)]
        public string? Director { get; set; }

        [StringLength(255)]
        [Column("movie_cast")]
        public string? Cast { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [Column("poster_image_url")]
        public string? PosterImageUrl { get; set; }

        [Column("trailer_url")]
        public string? TrailerUrl { get; set; }

        [Required]
        public MovieRating Rating { get; set; }

        // Navigation properties
        public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }

    public enum MovieGenre
    {
        ACTION,
        ADVENTURE,
        ANIMATION,
        COMEDY,
        CRIME,
        DOCUMENTARY,
        DRAMA,
        FAMILY,
        FANTASY,
        HORROR,
        MUSICAL,
        MYSTERY,
        ROMANCE,
        SCI_FI,
        THRILLER,
        WESTERN
    }

    public enum MovieRating
    {
        G,
        PG,
        PG13,
        R,
        NC17,
        UNRATED
    }
}