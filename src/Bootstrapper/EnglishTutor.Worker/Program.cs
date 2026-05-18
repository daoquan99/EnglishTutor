using EnglishTutor.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config =>
    config.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddWorkerServices(builder.Configuration);

var host = builder.Build();

Log.Information("EnglishTutor Worker started");

host.Run();
