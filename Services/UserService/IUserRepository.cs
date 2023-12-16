using livestock_api.Models;

namespace livestock_api.Services.UserService
{
    public interface IUserRepository
    {
        /* we define our methods here*/
        Task<Users> Register(Users newUser, string[] roles);
        Task<Users> FindByEmailAsync(string email);
        Task<string[]> GetRolesAsync(Users model);
    }
}
