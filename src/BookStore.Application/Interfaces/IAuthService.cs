using System.Threading.Tasks;
using BookStore.Application.DTOs;

namespace BookStore.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResultDto> LoginAsync(LoginDto loginDto);
    // Optionally: Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
} 