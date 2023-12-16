using System.Data;

namespace livestock_api.Models
{
    public class UserRoles
    {
        public int UserId { get; set; }
        public Users User { get; set; }  // Navigation property to the User entity

        public int RoleId { get; set; }
        public Role Role { get; set; } // Navigation property to the Role entity
    }
}
