using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.AuthDTOs;

public class PasswordChangeDTOs
{
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;

    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = null!;
}
