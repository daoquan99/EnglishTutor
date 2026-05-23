var builder = DistributedApplication.CreateBuilder(args);

var jwtSecret = builder.AddParameter("jwt-secret", secret: true);
var seedAdminPassword = builder.AddParameter("seed-admin-password", secret: true);

var database = builder.AddConnectionString("DefaultConnection");
var redis = builder.AddConnectionString("Redis");

var api = builder.AddProject<Projects.EnglishTutor_Api>("api")
    .WithReference(database)
    .WithReference(redis)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("SeedData__Enabled", "true")
    .WithEnvironment("SeedData__Admin__Password", seedAdminPassword);

builder.AddProject<Projects.EnglishTutor_Worker>("worker")
    .WithReference(database)
    .WithReference(redis)
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WaitFor(api);

builder.Build().Run();
