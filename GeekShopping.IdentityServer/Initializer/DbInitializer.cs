using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MySQLContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly RoleManager<IdentityRole> _role;

        public DbInitializer(MySQLContext context,
            UserManager<ApplicationUser> user,
            RoleManager<IdentityRole> role)
        {
            _context = context;
            _user = user;
            _role = role;
        }

        public void Initialize()
        {
            if (_role.FindByNameAsync(IdentityConfiguration.Admin).Result is null)
            {
                _role.CreateAsync(new IdentityRole(
                    IdentityConfiguration.Admin)).GetAwaiter().GetResult();
                _role.CreateAsync(new IdentityRole(
                    IdentityConfiguration.Client)).GetAwaiter().GetResult();
            }

            ApplicationUser admin = new ApplicationUser()
            {
                UserName = "admin",
                Email = "admin@admin.com",
                EmailConfirmed = true,
                PhoneNumber = "+55 (51) 12345-6789",
                FirstName = "Master",
                LastName = "Admin"
            };

            if (_user.FindByNameAsync(admin.UserName).Result is null)
            {
                _user.CreateAsync(admin, "Admin123$").GetAwaiter().GetResult();
                _user.AddToRoleAsync(admin,
                    IdentityConfiguration.Admin).GetAwaiter().GetResult();
                var adminClaims = _user.AddClaimsAsync(admin, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                    new Claim(JwtClaimTypes.GivenName, admin.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, admin.LastName),
                    new Claim(JwtClaimTypes.Role, IdentityConfiguration.Admin)
                }).Result;
            }

            ApplicationUser client = new ApplicationUser()
            {
                UserName = "irineus",
                Email = "irineus@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "+55 (51) 98418-2581",
                FirstName = "Irineu",
                LastName = "Santos"
            };

            if (_user.FindByNameAsync(client.UserName).Result is null)
            {
                _user.CreateAsync(client, "Irineu123$").GetAwaiter().GetResult();
                _user.AddToRoleAsync(client,
                    IdentityConfiguration.Client).GetAwaiter().GetResult();
                var clientClaims = _user.AddClaimsAsync(client, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
                    new Claim(JwtClaimTypes.GivenName, client.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, client.LastName),
                    new Claim(JwtClaimTypes.Role, IdentityConfiguration.Client)
                }).Result;
            }
        }
    }
}
