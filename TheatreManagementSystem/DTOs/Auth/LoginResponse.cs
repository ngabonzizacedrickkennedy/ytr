namespace TheatreManagementSystem.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;

        public LoginResponse()
        {
        }

        public LoginResponse(string token, string username, string roles)
        {
            Token = token;
            Username = username;
            Roles = roles;
        }
    }
}
