using TeacherSuite.Application.AgeGroups.Commands;
using TeacherSuite.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAgeGroupCommand).Assembly));
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();