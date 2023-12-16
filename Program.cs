global using livestock_api.Models;
global using Microsoft.EntityFrameworkCore;
global using livestock_api.Data;
global using Amazon.CognitoIdentityProvider.Model;
global using Amazon.CognitoIdentityProvider;
global using Amazon.Extensions.CognitoAuthentication;
using livestock_api.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using livestock_api.Services.AWSservice;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//when ever user controller wants to inject IUsers it should use UsersService
builder.Services.AddDbContext<MyDbContext>();

// Authentication configuration - AWS Cognito

/*
The first line adds Cognito services to the dependency injection container.
This allows the application to use Cognito APIs for user authentication and authorization.
The next block of code configures the authentication options by setting the default authentication
and challenge schemes to JWT Bearer authentication.
*/

builder.Services.AddCognitoIdentity();
builder.Services.AddScoped<IAWSUserRepository, AWSUserRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.Authority = builder.Configuration["Cognito:Authority"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidAudience = configuration["JWT:Audience"],
        ValidIssuer = configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();