namespace BookStore.Application.DTOs;

public class AuthResultDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public IEnumerable<string>? Errors { get; set; }
} 