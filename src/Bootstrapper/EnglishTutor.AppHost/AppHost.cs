var builder = DistributedApplication.CreateBuilder(args);

var jwtSecret = builder.AddParameter("jwt-secret", secret: true);
var seedAdminPassword = builder.AddParameter("seed-admin-password", secret: true);

var postgresServer = builder.AddPostgres("postgres")
    .WithDataVolume();

var database = postgresServer.AddDatabase("english-tutor-db", "english_tutor_db");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var api = builder.AddProject<Projects.EnglishTutor_Api>("api")
    .WithReference(database, "DefaultConnection")
    .WithReference(redis, "Redis")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("SeedData__Enabled", "true")
    .WithEnvironment("SeedData__Admin__Password", seedAdminPassword)
    .WaitFor(database)
    .WaitFor(redis);

builder.AddProject<Projects.EnglishTutor_Worker>("worker")
    .WithReference(database, "DefaultConnection")
    .WithReference(redis, "Redis")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WaitFor(database)
    .WaitFor(redis)
    .WaitFor(api);

builder.Build().Run();
