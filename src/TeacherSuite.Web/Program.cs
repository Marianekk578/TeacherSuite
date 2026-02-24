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
builder.Services.AddScoped<Groups>();
builder.Services.AddScoped<ProgrammingLanguages>();

var app = builder.Build();

app.UseGlobalExceptionHandler();
if (app.Environment.IsDevelopment()) {
    app.UseRequestLogging();
}

app.MapAgeGroupEndpoints();
app.MapTeacherEndpoints();
app.MapCourseEndpoints();
app.MapGroupEndpoints();
app.MapProgrammingLanguageEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
