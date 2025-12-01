using MediatR;
using TeacherSuite.Application.AgeGroups.Commands;
using TeacherSuite.Application.AgeGroups.Queries;
using TeacherSuite.Application.ProgrammingLanguages.Commands;
using TeacherSuite.Application.ProgrammingLanguages.Queries;
using TeacherSuite.Infrastructure;
using TeacherSuite.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddScoped<AgeGroups>();
builder.Services.AddScoped<ProgrammingLanguages>();

var app = builder.Build();

app.MapGet("/AgeGroups", async (AgeGroups endpoints, ISender sender, [AsParameters] GetAgeGroupsQuery query) =>
    await endpoints.GetAgeGroups(sender, query));

app.MapPost("/AgeGroups", async (AgeGroups endpoints, ISender sender, CreateAgeGroupCommand command) =>
    await endpoints.CreateAgeGroups(sender, command));

app.MapGet("/ProgrammingLanguages", async (ProgrammingLanguages endpoints, ISender sender, [AsParameters] GetProgrammingLanguagesQuery query) =>
    await endpoints.GetProgrammingLanguages(sender, query));

app.MapGet("/ProgrammingLanguages/{id}", async (ProgrammingLanguages endpoints, ISender sender, int id) =>
    await endpoints.GetProgrammingLanguageById(sender, id));

app.MapPost("/ProgrammingLanguages", async (ProgrammingLanguages endpoints, ISender sender, CreateProgrammingLanguageCommand command) =>
    await endpoints.CreateProgrammingLanguage(sender, command));

app.MapPut("/ProgrammingLanguages/{id}", async (ProgrammingLanguages endpoints, ISender sender, int id, UpdateProgrammingLanguageCommand command) =>
    await endpoints.UpdateProgrammingLanguage(sender, id, command));

app.MapDelete("/ProgrammingLanguages/{id}", async (ProgrammingLanguages endpoints, ISender sender, int id) =>
    await endpoints.DeleteProgrammingLanguage(sender, id));


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();