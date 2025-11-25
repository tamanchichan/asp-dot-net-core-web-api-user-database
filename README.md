# ASP.NET CORE WEB API USER DATABASE
Created a simple ASP.NET Core Web API project for managing a user database.
The application exposes endpoints to create, read, update, and delete (CRUD)
user records using Entity Framework Core with SQL Server Express. It supports
running the database locally or through a Docker container, making it easy to
develop and deploy across different environments.

**Connection string is set up to connect to SQL Server Express running in
a Docker container.**

## Setting Up ASP.NET Core Identity with Entity Framework Core
1. Add a new scaffolded item to your project:
    - Right click on the project in Solution Explorer.
    - Add > New Scaffolded Item...
    - Identity > Identity
    - (Optional) Choose a layout page if you have one, otherwise leave it blank.

2. Create a new Data Context:
    - For example, "ApplicationDbContext"
    - Either select your existing database or create a new one.      
3. (Optional) Extend IdentityUser:
    - Create a custom User class if you need additional properties.Create a User class if you want to extend the default IdentityUser.


4. Apply migrations:
    - Open Package Manager Console.
    - Run Add-Migration InitialCreate to create the initial migration.
    - Run Update-Database to apply the migration to your database.

5. (Optional) Seed initial data:
    - Create a SeedData class.
    - Use UserManager and RoleManager to create default users and roles (e.g., an admin account).

## Running SQL Server Express in Docker
1. Pull the SQL Server Docker Image:
```bash
docker pull mcr.microsoft.com/mssql/server:2019-latest
```

2. Run the SQL Server Express Container:
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=SqlExpress@123" -e "MSSQL_PID=Express" -p 1433:1433 -d --name sql-express-container mcr.microsoft.com/mssql/server:2019-latest
```
- Replace `YourStrong!Passw0rd` with a secure password of your choice.
- The `-d` option runs the container in detached mode.
- The `MSSQL_PID=Express` environment variable specifies that you want to run the Express edition of SQL Server.
    - Without it, SQL Server defaults to Developer edition in the 2019-latest image.
    - Using Express ensures you’re using the free, limited version (max 10GB DB, limited CPU/RAM).


3. Connect to SQL Express Server:
- Use SQL Server Management Studio (SSMS) or any SQL client to connect to `localhost,1433` with:
    - Username: `sa`
    - Password: `SqlExpress@123` (or your chosen password)
or
- Use the connection string in your ASP.NET Core application:
```csharp
"Server=host.docker.internal,1433;Database=UserDbContext;User Id=SA;Password=SqlExpress@123;TrustServerCertificate=True;"
```

### Running the Application with SQL Express in Docker:
Before starting your ASP.NET Core Web API, make sure the SQL Server Express
container is running. You can do this either via bash/terminal or manually
through Docker Desktop.
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=SqlExpress@123" -e "MSSQL_PID=Express" -p 1433:1433 -d --name sql-express-container mcr.microsoft.com/mssql/server:2019-latest
```

1. (Optional) Create/Configue Dockerfile for ASP.NET Core Application:

2. Build the ASP.NET Core Application Docker Image:
```bash
docker build -t user-database-image .
```

3. Build and Run the ASP.NET Core Application Container:
```bash
docker run -d -p 8080:8080 --name user-database-container user-database-image
```
