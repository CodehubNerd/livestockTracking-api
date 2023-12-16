using livestock_api.Models;

namespace livestock_api.Services.AWSservice
{
    public interface IAWSUserRepository
    {
        Task<RegistrationReponse> CreateUserAsync(Users newUser);
        Task<ConfirmRegisterResponse> ConfirmUserSignUpAsyc(ConfirmRegister confirm);
        Task<AuthResponse> TryLoginAsync(LoginRequest model);

    }
}
