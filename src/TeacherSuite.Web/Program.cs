using TeacherSuite.Infrastructure;
using TeacherSuite.Web.Endpoints;
using TeacherSuite.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddScoped<AgeGroups>();
builder.Services.AddScoped<Teachers>();
builder.Services.AddScoped<Courses>();

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseRequestLogging();

app.MapAgeGroupEndpoints();
app.MapTeacherEndpoints();
app.MapCourseEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
