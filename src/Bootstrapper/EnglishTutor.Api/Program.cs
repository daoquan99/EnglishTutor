using EnglishTutor.Api;
using EnglishTutor.Api.Middlewares;
using EnglishTutor.Modules.AI.Presentation;
using EnglishTutor.Modules.Auth.Presentation;
using EnglishTutor.Modules.Mistakes.Presentation;
using EnglishTutor.Modules.Progress.Presentation;
using EnglishTutor.Modules.Speaking.Presentation;
using EnglishTutor.Modules.Users.Presentation;
using EnglishTutor.Modules.Vocabulary.Presentation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapAiEndpoints();
app.MapVocabularyEndpoints();
app.MapSpeakingEndpoints();
app.MapMistakeEndpoints();
app.MapProgressEndpoints();

app.Run();
