using Nox;
using Samples.Api.Domain.Store.Commands;
using Samples.Api.Domain.Store.Queries;

namespace Samples.Api.Config
{
    public static class NoxDomainConfiguration
    {
        public static void AddServices(IServiceCollection services)
        {
            services.AddDbContext<NoxDomainDbContext>();

            services.AddScoped<Nox.Queries.ActiveReservationsQuery, ActiveReservations>();
            services.AddScoped<Nox.Commands.ExchangeCommandHandlerBase, ExchangeCommandHandler>();
            services.AddScoped<Nox.Commands.ReserveCommandHandlerBase, ReserveCommandHandler>();
        }

        public static void ConfigureEndpoints(WebApplication app)
        {
            app.MapGet("api/Stores/{storeId}/activeReservations",
                async (int storeId) =>
                {
                    using var scope = app.Services.CreateScope();
                    await scope.ServiceProvider
                        .GetRequiredService<Nox.Queries.ActiveReservationsQuery>()
                        .ExecuteAsync(storeId);
                });

            app.MapPost("api/Stores/{storeId}/exchange",
                async (int storeId, Nox.Commands.ExchangeCommand exchangeCommandDto) =>
                {
                    using var scope = app.Services.CreateScope();
                    await scope.ServiceProvider
                        .GetRequiredService<Nox.Commands.ExchangeCommandHandlerBase>()
                        .ExecuteAsync(exchangeCommandDto);
                });

            app.MapPost("api/Stores/{storeId}/reserve",
                async (int storeId, Nox.Commands.ReserveCommand reserveDto) =>
                {
                    using var scope = app.Services.CreateScope();
                    await scope.ServiceProvider
                        .GetRequiredService<Nox.Commands.ReserveCommandHandlerBase>()
                        .ExecuteAsync(reserveDto);
                });
        }
    }
}
