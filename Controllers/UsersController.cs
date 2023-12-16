using livestock_api.Services.AWSservice;
using livestock_api.Services.UserService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace livestock_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class UsersController : ControllerBase
    {
        //Injecting the service 
        private readonly IAWSUserRepository _awsRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

       

        //our constructor
        public UsersController(IUserRepository UserRepository, IAWSUserRepository AWSUserRepository, IConfiguration configuration)
        {
            _userRepository = UserRepository;
            _awsRepository = AWSUserRepository;
            _configuration = configuration;

        }


        //user registration end-point
        [HttpPost("register")]
        public async Task<ActionResult> Register(Users newuser)
        {
            if (string.IsNullOrEmpty(newuser?.Email) && string.IsNullOrEmpty(newuser.Password))
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error",
                    Message = "Something went wrong, Fill all spaces"});
         

        Users userExists = await _userRepository.FindByEmailAsync(newuser.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response
                      {
                        Status = "Error",
                        Message = "Email already exist!"
                       });

            try
            {
                var result = await _awsRepository.CreateUserAsync(newuser);
                if (!result.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new Response
                        {
                            Status = "Error",
                            Message = result.Message
                        });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = ex.Message 
                    });
            }

            Users users = new Users()
            {
                Name = newuser.Name,
                Email = newuser.Email,
                PhoneNumber = newuser.PhoneNumber, 
                Password = newuser.Password,
                Profile = newuser.Profile,
                Picture = newuser.Picture,
                Website = newuser.Website,
                Gender = newuser.Gender,
                Birthdate = newuser.Birthdate,
                Address = newuser.Address
            };

            await _userRepository.Register(newuser, newuser.UserRoles.Select(ur => ur.Role.Name).ToArray());

            return Ok(new Response
            {
                Status = "Success",
                Message = "Registration Successful! Verification code sent to your email"
            });
        }

        //confirm verification end-point
        [HttpPost("confirm-register")]
        public async Task<ActionResult<string>> ConfirmRegister(ConfirmRegister confirm)
        {
            var result = await _awsRepository.ConfirmUserSignUpAsyc(confirm);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginRequest users)
        {
            if (string.IsNullOrEmpty(users.Name))
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Login failed! expecting user email on sign in." });
                var result = await _awsRepository.TryLoginAsync(users);
            if(!result.IsSuccess)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Login failed! Email not found." });
            Users user = await _userRepository.FindByEmailAsync(users.Name);
            if(user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Login failed! Please check user details and try again." });
            }
            var Roles = await _userRepository.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            if (Roles != null)
            {
                foreach (var Role in Roles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, Role));
                }
                var token = GetToken(authClaims);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
             return Unauthorized();
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            string secret = _configuration["JWT:SecretKey"];
            byte[] secretByte = Encoding.UTF8.GetBytes(secret);
            var authSigningKey = new SymmetricSecurityKey(secretByte);
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return token;
        }
    }
}
