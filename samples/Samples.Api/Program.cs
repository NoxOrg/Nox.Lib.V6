using System.Reflection;
using MassTransit;
using Nox.Messaging;
using Nox.Microservice.Extensions;
using Samples.Api.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Nox to the service collection
builder.Services.AddNox();
builder.Services.AddMediator(cfg =>
{
    cfg.AddConsumer<CountryCreatedEventConsumer>();
});

builder.Services.AddNoxEvents(Assembly.GetExecutingAssembly());

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

app.Run();