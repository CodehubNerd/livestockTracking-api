namespace livestock_api.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
    }
}
