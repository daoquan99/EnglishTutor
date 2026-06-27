var builder = DistributedApplication.CreateBuilder(args);

var environment = Microsoft.Extensions.Hosting.RepositoryEnvironment.Load();

var api = builder.AddProject<Projects.EnglishTutor_Api>("api");
foreach (var (key, value) in environment)
{
    api.WithEnvironment(key, value);
}

var worker = builder.AddProject<Projects.EnglishTutor_Worker>("worker");
foreach (var (key, value) in environment)
{
    worker.WithEnvironment(key, value);
}

builder.Build().Run();
