namespace PRS.DTOs
{
    public class ResetPasswordDTO
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
