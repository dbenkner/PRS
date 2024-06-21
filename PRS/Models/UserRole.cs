using System.Text.Json.Serialization;

namespace PRS.Models
{
    public class UserRole
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public Role Role { get; set; }
    }
}
