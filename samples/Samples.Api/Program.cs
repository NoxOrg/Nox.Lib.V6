using Nox;
using Samples.Api.Config;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Nox to the service collection
builder.Services.AddNox();

// Register queries/commands implementations - TODO: make automatic
NoxDomainConfiguration.AddServices(builder.Services);

var app = builder.Build();

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

// Register queries/commands endpoints - TODO: make automatic according to *.api.nox.yaml
NoxDomainConfiguration.ConfigureEndpoints(app);

app.Run();