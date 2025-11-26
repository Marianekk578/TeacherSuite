using MediatR;
using TeacherSuite.Application.AgeGroups.Commands;
using TeacherSuite.Application.AgeGroups.Queries;
using TeacherSuite.Application.Teachers.Commands.Create;
using TeacherSuite.Application.Teachers.Commands.Update;
using TeacherSuite.Application.Teachers.Queries.Get;
using TeacherSuite.Infrastructure;
using TeacherSuite.Web.Endpoints;
using TeacherSuite.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddScoped<AgeGroups>();
builder.Services.AddScoped<Teachers>();

var app = builder.Build();

app.MapGet("/AgeGroups", async (AgeGroups endpoints, ISender sender, [AsParameters] GetAgeGroupsQuery query) =>
    await endpoints.GetAgeGroups(sender, query));

app.MapPost("/AgeGroups", async (AgeGroups endpoints, ISender sender, CreateAgeGroupCommand command) =>
    await endpoints.CreateAgeGroups(sender, command));

app.MapPost("/Teachers", async (Teachers endpoints, ISender sender, CreateTeacherCommand command) =>
    await endpoints.CreateTeachers(sender, command));

app.MapGet("/Teachers/assigned", async (Teachers endpoints, ISender sender, [AsParameters] GetTeacherAssignedToGroupQuery query) =>
    await endpoints.GetTeacherAssignedToGroup(sender, query));

app.MapPut("/Teachers/{id:guid}", async (Teachers endpoints, ISender sender, Guid id, UpdateTeacherCommand command) =>
    await endpoints.UpdateTeacher(sender, id, command));

app.MapGet("/Teachers", async (Teachers endpoints, ISender sender, [AsParameters] GetAllTeachersQuery query) =>
    await endpoints.GetAllTeachers(sender, query));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRequestLogging();

app.Run();