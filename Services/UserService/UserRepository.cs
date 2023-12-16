using livestock_api.Data;
using livestock_api.Models;

namespace livestock_api.Services.UserService

{
    public class UserRepository : IUserRepository
    {  /*Injecting a service*/
        private readonly MyDbContext _dbContext;

        //our constractor for db context
        public UserRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Users> Register(Users newUser, string[] roles)
        {
            var result = await _dbContext.Users.AddAsync(newUser);

            // Set roles for the added user
            if (roles != null && roles.Length > 0)
            {
                foreach (var role in roles)
                {
                    var roleEntity = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == role);
                    if (roleEntity != null)
                    {
                        var userRole = new UserRoles
                        {
                            UserId = result.Entity.Id,
                            RoleId = roleEntity.Id
                        };
                        _dbContext.UserRoles.Add(userRole);
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }


        /*Checking email existence in our DB*/
        public async Task<Users> FindByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(e => e.Email == email); 
        }
        public async Task<string[]> GetRolesAsync(Users model)
        {
            var user = await _dbContext.Users
                 .Include(u => u.UserRoles) // Include the UserRoles navigation property
                 .ThenInclude(ur => ur.Role) // Include the Role navigation property within UserRoles
                 .FirstOrDefaultAsync(e => e.Email == model.Email);

            var roles = user?.UserRoles.Select(ur => ur.Role.Name).ToArray() ?? Array.Empty<string>();
            return roles;
        }



    }
}
