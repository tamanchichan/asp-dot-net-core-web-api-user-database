using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace asp_dot_net_core_web_api_users_database.Areas.Identity.Data
{
    public static class SeedData
    {
        public async static Task Initialize(IServiceProvider serviceProvider)
        {
            UserDbContext context = new UserDbContext(serviceProvider.GetRequiredService<DbContextOptions<UserDbContext>>());
            //await context.Database.EnsureCreatedAsync();

            UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
                "Admin",
                "User"
            };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string adminUserName = "admin";
            string adminEmail = "admin@email.com";
            string adminPassword = "admin";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                IdentityUser adminUser = new IdentityUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, roles[0]);
                }
                else
                {
                    Console.WriteLine("Error creating admin user:");
                }
            }
        }
    }
}
