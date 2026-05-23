using EnglishTutor.Worker;
using EnglishTutor.Worker.Extensions;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config =>
    config.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddWorkerServices(builder.Configuration);

var host = builder.Build();

await host.MigrateWorkerDatabasesAsync();

Log.Information("EnglishTutor Worker started");

await host.RunAsync();
