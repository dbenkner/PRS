using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PRS.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }
        [StringLength(30)]
        public string Username { get; set; } = string.Empty;
        [StringLength(255)]
        [JsonIgnore]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        [StringLength(255)]
        [JsonIgnore]
        public byte[] PasswordHash { get; set;} = Array.Empty<byte>();
        [StringLength(30)]
        public string Firstname { get; set; } = string.Empty;
        [StringLength(30)]
        public string Lastname { get; set; } = string.Empty;
        [StringLength(12)]
        public string? Phone { get; set; } = string.Empty;
        [StringLength(255)]
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;
        public virtual List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
