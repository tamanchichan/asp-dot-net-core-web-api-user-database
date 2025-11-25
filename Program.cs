using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using asp_dot_net_core_web_api_users_database.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Get the connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("UserDbContextConnection") ?? throw new InvalidOperationException("Connection string 'UserDbContextConnection' not found."); ;

// Add DbContext services
builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(connectionString));

// Add Identity services
builder.Services.AddDefaultIdentity<IdentityUser>
    (options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
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
app.MapGet("/users", (UserDbContext context) =>
{
    //List<IdentityUser> users = context.Users.ToList();

    // Create a DTO (UserDTO) or anonymous type (var) to avoid exposing sensitive information
    // Select only specific fields to return
    var users = context.Users
        .Select(user => new { user.Id, user.UserName, user.Email })
        .ToList();

    if (users == null || users.Count == 0)
    {
        return Results.NotFound("No users found.");
    }

    return Results.Ok(users);
});

app.Run();