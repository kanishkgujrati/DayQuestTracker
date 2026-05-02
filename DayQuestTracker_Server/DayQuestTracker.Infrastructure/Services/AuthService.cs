using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Application.Features.Auth;
using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DayQuestTracker.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly TrackerDBContext _dbcontext;
        private readonly IAuthTokenGeneratorService _authTokenGeneratorService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly AuthConfiguration _authConfiguration;
        public AuthService(TrackerDBContext context, IAuthTokenGeneratorService authTokenGenerator, IPasswordHasher passwordHasher, IOptions<AuthConfiguration> authConfiguration)
        {
            _dbcontext = context;
            _authTokenGeneratorService = authTokenGenerator;
            _passwordHasher = passwordHasher;
            _authConfiguration = authConfiguration.Value;
        }

        public async Task<AuthResponse> RegisterAsync(UserRequest request)
        {
            // Check if email already exists
            if (await _dbcontext.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email already registered.");

            // Check if username already exists
            if (await _dbcontext.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("Username already taken.");

            var refreshToken = _authTokenGeneratorService.GenerateRefreshToken();

            var user = new User
            {
                Email = request.Email.ToLower().Trim(),
                Username = request.Username.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Timezone = request.Timezone,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(_authConfiguration.RefreshTokenExpiryDays)
            };

            _dbcontext.Users.Add(user);
            await _dbcontext.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = _authTokenGeneratorService.GenerateAccessToken(user),
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_authConfiguration.AccessTokenExpiryMinutes),
                UserId = user.Id,
                Username = user.Username,
                Level = user.Level,
                TotalXP = user.TotalXP
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Always say "invalid credentials" — never reveal if email exists
            // Why: telling a user "email not found" helps attackers enumerate accounts
            var user = await _dbcontext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            var refreshToken = _authTokenGeneratorService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_authConfiguration.RefreshTokenExpiryDays);
            user.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = _authTokenGeneratorService.GenerateAccessToken(user),
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_authConfiguration.AccessTokenExpiryMinutes),
                UserId = user.Id,
                Username = user.Username,
                Level = user.Level,
                TotalXP = user.TotalXP
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var user = await _dbcontext.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var newRefreshToken = _authTokenGeneratorService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_authConfiguration.RefreshTokenExpiryDays);
            user.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = _authTokenGeneratorService.GenerateAccessToken(user),
                RefreshToken = newRefreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_authConfiguration.AccessTokenExpiryMinutes),
                UserId = user.Id,
                Username = user.Username,
                Level = user.Level,
                TotalXP = user.TotalXP
            };
        }
    }
}
