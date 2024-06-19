using System.ComponentModel.DataAnnotations;

namespace PRS.DTOs
{
    public class NewUserDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [EmailAddress]
        public string? Email { get; set; }
        [Phone]
        public string? Phone { get; set; }

    }
}
