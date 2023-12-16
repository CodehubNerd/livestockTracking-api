namespace livestock_api.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Profile { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Birthdate { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Navigation property for user roles
        public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();

    }
}
