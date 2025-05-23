namespace TheatreManagementSystem.Security
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string Secret { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 1440; // 24 hours
        public string Issuer { get; set; } = "TheatreManagementSystem";
        public string Audience { get; set; } = "TheatreManagementSystemUsers";
    }
}