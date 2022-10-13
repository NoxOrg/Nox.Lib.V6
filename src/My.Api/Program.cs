
using Nox.Dynamic.Extensions;

var builder = WebApplication.CreateBuilder(args);

/* TODO: Lakkana
2. Add a SqlKata compiler to Nox.....IDatabaseProvider : implement for sqlserver and postgres
1. Finish the PostgresLoaderProvider as a generic loader using sqlkata
3. Move sortorder setting to validation of Service.cs in MetaData
*/

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add magic
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











