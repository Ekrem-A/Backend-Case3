using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Models.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
