using Microsoft.OpenApi.Models;
using Nox;
using Nox.Api.OData.Swagger;
using Nox.Core.Interfaces.Configuration;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(cfg =>
{
    //Add this to ensure swagger document is correctly annotated
    cfg.EnableAnnotations();

    cfg.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "0.01",
        Title = "Nox Sample API"
    });

    cfg.DocumentFilter<ODataCustomSwaggerFilter>();
});

// Add Nox to the service collection
builder.Services.AddNox();

var app = builder.Build();

ODataCustomSwaggerFilter.SetProjectConfiguration(app.Services.GetService<IProjectConfiguration>());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Add Nox to the application middleware
app.UseNox();

app.UseAuthorization();

app.MapControllers();

app.Run();