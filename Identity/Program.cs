
using System.Security.Claims;
using System.Text;
using Identity.Data.Context;
using Identity.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            #region Database
            var connectionString = builder.Configuration.GetConnectionString("default");
            builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(connectionString));
            #endregion

            #region Identity
            builder.Services.AddIdentityCore<CustomUser>(options =>
            {
                // Validation to be read from configurations
                options.Password.RequiredUniqueChars = 2;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;

                options.User.RequireUniqueEmail = true;
            })
               .AddEntityFrameworkStores<UserDbContext>();
            #endregion

            #region Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
                {
                    var secretKey = builder.Configuration.GetValue<string>("SecretKey")!;
                    var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
                    var key = new SymmetricSecurityKey(secretKeyBytes);

                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = key
                    };
                });
            #endregion

            #region Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    Constants.Policies.Admin,
                    builder => builder
                        .RequireClaim(ClaimTypes.Role, "Admin")
                        .RequireClaim(ClaimTypes.NameIdentifier)
                );

                options.AddPolicy(
                    Constants.Policies.User,
                    builder => builder
                        .RequireClaim(ClaimTypes.Role, "customValue")
                        .RequireClaim(ClaimTypes.NameIdentifier)
                );

            });
            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            //auth
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
