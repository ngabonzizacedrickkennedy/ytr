using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TheatreManagementSystem.Security
{
    public class JwtTokenProvider
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenProvider(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        // Generate JWT token
        public string GenerateToken(ClaimsPrincipal principal)
        {
            var username = principal.Identity?.Name ?? throw new ArgumentException("Username is required");
            var currentDate = DateTime.UtcNow;
            var expiryDate = currentDate.AddMinutes(_jwtSettings.ExpirationMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username),
                new(JwtRegisteredClaimNames.Sub, username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(currentDate).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add role claims
            var roles = principal.FindAll(ClaimTypes.Role);
            claims.AddRange(roles);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: currentDate,
                expires: expiryDate,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Generate token based on username
        public string GenerateTokenFromUsername(string username)
        {
            var currentDate = DateTime.UtcNow;
            var expiryDate = currentDate.AddMinutes(_jwtSettings.ExpirationMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username),
                new(JwtRegisteredClaimNames.Sub, username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(currentDate).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: currentDate,
                expires: expiryDate,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private SymmetricSecurityKey GetKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        }

        // Extract username from JWT token
        public string GetUsernameFromToken(string token)
        {
            return GetClaimFromToken(token, ClaimTypes.Name);
        }

        // Extract claim from token
        public string GetClaimFromToken(string token, string claimType)
        {
            var claims = GetAllClaimsFromToken(token);
            return claims.FirstOrDefault(c => c.Type == claimType)?.Value ?? string.Empty;
        }

        // Get all claims from token
        private IEnumerable<Claim> GetAllClaimsFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetKey();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal.Claims;
            }
            catch
            {
                return Enumerable.Empty<Claim>();
            }
        }

        // Check if token is expired
        public bool IsTokenExpired(string token)
        {
            var expirationTime = GetExpirationDateFromToken(token);
            return expirationTime < DateTime.UtcNow;
        }

        // Get expiration date from token
        public DateTime GetExpirationDateFromToken(string token)
        {
            var expClaim = GetClaimFromToken(token, JwtRegisteredClaimNames.Exp);
            if (long.TryParse(expClaim, out var exp))
            {
                return DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
            }
            return DateTime.MinValue;
        }

        // Validate JWT token
        public bool ValidateToken(string token, ClaimsPrincipal userPrincipal)
        {
            var username = GetUsernameFromToken(token);
            return username == userPrincipal.Identity?.Name && !IsTokenExpired(token);
        }

        // Validate token
        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = GetKey();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (SecurityTokenMalformedException)
            {
                throw new ArgumentException("Invalid JWT token");
            }
            catch (SecurityTokenExpiredException)
            {
                throw new ArgumentException("Expired JWT token");
            }
            catch (SecurityTokenNotYetValidException)
            {
                throw new ArgumentException("JWT token not yet valid");
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                throw new ArgumentException("Invalid JWT token audience");
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                throw new ArgumentException("Invalid JWT token issuer");
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                throw new ArgumentException("Invalid JWT signature");
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("JWT claims string is empty");
            }
            catch
            {
                return false;
            }
        }
    }
}