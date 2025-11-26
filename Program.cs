using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using asp_dot_net_core_web_api_users_database.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Get the connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("UserDbContextConnectionString") ?? throw new InvalidOperationException("Connection string 'UserDbContextConnection' not found."); ;

// Add DbContext services
builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(connectionString));

// Add Identity services
builder.Services.AddDefaultIdentity<IdentityUser>
    (options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 3;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<UserDbContext>();

// Add services to the container.
var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;

    await SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapPost("/users/create", async (UserDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, string roleName, string userName, string email, string password) =>
{
    try
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            throw new Exception(roleName);
        }

        IdentityUser newUser = new IdentityUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };

        IdentityResult result = await userManager.CreateAsync(newUser, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newUser, roleName);

            return Results.Ok(new
            {
                newUser.Id,
                newUser.UserName,
                newUser.Email
            });
        }
        else
        {
            return Results.BadRequest(result.Errors);
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGet("/", () =>
{
    return Results.Ok();
});

// Retrieve users from the database
app.MapGet("/users", (UserDbContext context, string? email) =>
{
    try
    {
        if (!string.IsNullOrEmpty(email))
        {
            IdentityUser user = context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                throw new ArgumentOutOfRangeException(email, "User with the specified email not found.");
            }

            return Results.Ok(new
            {
                user.Id,
                user.UserName,
                user.Email
            });
        }
        else
        {
            //List<IdentityUser> users = context.Users.ToList();

            // Create a DTO (UserDTO) or anonymous type (var) to avoid exposing sensitive information
            // Select only specific fields to return
            var users = context.Users
                .Select(user => new { user.Id, user.UserName, user.Email })
                .ToList();

            if (users == null || users.Count == 0)
            {
                throw new ArgumentNullException(nameof(users), "No users found in the database.");
            }

            return Results.Ok(users);
        }
    }
    catch (ArgumentNullException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPut("/users/edit/user={email}", (UserDbContext context, string email, string userName) =>
{
    try
    {
        IdentityUser user = context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            return Results.NotFound(user);
        }

        if (String.IsNullOrEmpty(userName))
        {
            throw new ArgumentNullException(userName, "Username cannot be null or empty.");
        }

        user.UserName = userName;
        context.SaveChangesAsync();

        return Results.Ok(new
        {
            user.Id,
            user.UserName,
            user.Email
        });
    }
    catch (ArgumentNullException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();