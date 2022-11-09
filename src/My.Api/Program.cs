using FO.Consumer;
using Nox.Dynamic.Extensions;
using Nox.Dynamic.Loaders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add magic
builder.Services.RegisterNoxConsumers(new List<INoxConsumer> { new INT001_Customer() });
builder.Services.AddNox();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseNox();

app.UseAuthorization();

app.MapControllers();

app.Run();











