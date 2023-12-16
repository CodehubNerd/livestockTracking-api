using Amazon;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using livestock_api.Models;

namespace livestock_api.Services.AWSservice
{
    public class AWSUserRepository : IAWSUserRepository
    {
        private readonly AppConfig _cloudConfig;
        private readonly CognitoUserPool _cognitoUserPool;
        private readonly IAmazonCognitoIdentityProvider _cognitoIdentityProvider;

        public AWSUserRepository(IConfiguration configuration)
        {
            _cloudConfig = new AppConfig
            {
                AccessKeyId = configuration["Appconfig:AccessKeyId"],
                AppClientId = configuration["Appconfig:AppClientId"],
                AccessSecretKey = configuration["Appconfig:AccessSecretKey"],
                AWSRegion = configuration["Appconfig:AWSRegion"],
                UserPoolId = configuration["Appconfig:UserPoolId"],
            };

            _cognitoIdentityProvider = new AmazonCognitoIdentityProviderClient
                (
                _cloudConfig.AccessKeyId,
                _cloudConfig.AccessSecretKey,
                RegionEndpoint.GetBySystemName(_cloudConfig.AWSRegion)
                );

            _cognitoUserPool = new CognitoUserPool(
                _cloudConfig.UserPoolId,
                _cloudConfig.AppClientId,
                _cognitoIdentityProvider
                );
        }

        public async Task<RegistrationReponse> CreateUserAsync(Users newUser)
        {
            // create  RegistrationRequest        
            var registrationRequest = new SignUpRequest
            {
                ClientId = _cloudConfig.AppClientId,
                Password = newUser.Password,
                Username = newUser.Email
            };

            registrationRequest.UserAttributes.Add(new AttributeType
            {
                Name = "email",
                Value = newUser.Email
            });

            registrationRequest.UserAttributes.Add(new AttributeType
            {
                Name = "phone_number",
                Value = newUser.PhoneNumber
            });

            registrationRequest.UserAttributes.Add(new AttributeType
            {
                Name = "name",
                Value = newUser.Name
            });

            registrationRequest.UserAttributes.Add(new AttributeType
            {
                Name = "profile",
                Value = newUser.Profile
            });

            registrationRequest.UserAttributes.Add(new AttributeType
            {
                Name = "picture",
                Value = newUser.Picture
            });

            registrationRequest.UserAttributes.Add(new AttributeType
            {
                Name = "website",
                Value = newUser.Website
            });

            registrationRequest.UserAttributes.Add(new AttributeType
            {
                Name = "gender",
                Value = newUser.Gender
            });

            registrationRequest.UserAttributes.Add(new AttributeType
            {
                Name = "birthdate",
                Value = newUser.Birthdate
            });

            registrationRequest.UserAttributes.Add(new AttributeType
            {
                Name = "address",
                Value = newUser.Address
            });

            try
            {
                // call RegistrationAsync() method
                SignUpResponse? response = await _cognitoIdentityProvider.SignUpAsync(registrationRequest);

                var registrationResponse = new RegistrationReponse
                {
                    UserId = response.UserSub,
                    EmailAddress = newUser.Email,
                    Message = $"Registration Successful!! Confirmation Code sent to {response.CodeDeliveryDetails.Destination} via {response.CodeDeliveryDetails.DeliveryMedium.Value}",
                    IsSuccess = true
                };
                return registrationResponse;
            }
            catch (UsernameExistsException)
            {
                return new RegistrationReponse
                {
                    IsSuccess = false,
                    Message = "EmailAddress Already Exists"
                };
            }
        }
        //confirmation of user verifiaction code
        public async Task <ConfirmRegisterResponse> ConfirmUserSignUpAsyc(ConfirmRegister confirm)
        {
            ConfirmSignUpRequest request = new ConfirmSignUpRequest
            {
                ClientId = _cloudConfig.AppClientId,
                ConfirmationCode = confirm.ConfirmationCode,
                Username = confirm.UserName
            };

            try
            {
                var response = await _cognitoIdentityProvider.ConfirmSignUpAsync(request);
                return new ConfirmRegisterResponse
                {
                    Message = "User confirmed",
                    IsSuccess = true
                };
            }
            catch (CodeMismatchException)
            {
                return new ConfirmRegisterResponse
                {
                    IsSuccess = false,
                    Message = "Invalid confirmation Code,"
                };
            }

        }
        //login
        public async Task<AuthResponse> TryLoginAsync(LoginRequest model)
        {
            try
            {
                CognitoUser user = new CognitoUser(
                    model.Name,
                    _cloudConfig.AppClientId,
                    _cognitoUserPool,
                    _cognitoIdentityProvider);
                InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
                {
                    Password = model.Password,
                };
               
                AuthFlowResponse authResponse = await user.StartWithSrpAuthAsync(authRequest);
                var result = authResponse.AuthenticationResult;

                var authResponsemodel = new AuthResponse();
                authResponsemodel.EmailAddress = user.UserID;
                authResponsemodel.UserName = user.Username;
                authResponsemodel.IsSuccess = result.AccessToken != null;
                return authResponsemodel;
              
            }
            catch (UserNotConfirmedException)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Email not confirmed.",
                };
            }
            catch (UserNotFoundException)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Email not found.",

                };
            }
            catch (NotAuthorizedException e) 
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Incorrect username or passowrd",
                };
            }
            catch (Exception e)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Incorrect username or passowrd"
                };
            }
        }

    }
}
