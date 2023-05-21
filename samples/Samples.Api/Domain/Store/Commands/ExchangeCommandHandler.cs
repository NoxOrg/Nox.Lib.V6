using Microsoft.EntityFrameworkCore;
using Nox;
using Nox.Core.Interfaces.Entity.Commands;
using Nox.Core.Interfaces.Messaging;
using Nox.Commands;
using Nox.Dto;
using Nox.Core.Interfaces.Entity;

namespace Samples.Api.Domain.Store.Commands
{
    /// <summary>
    /// Performs an exchange operation for an existing reservation.
    /// Handler is derived from the automatically generated.
    /// </summary>
    public class ExchangeCommandHandler : ExchangeCommandHandlerBase
    {
        private readonly ILogger _logger;

        public ExchangeCommandHandler(NoxDomainDbContext dbContext, INoxMessenger messenger, ILogger<ExchangeCommandHandler> logger)
            : base(dbContext, messenger)
        {
            _logger = logger;
        }

        public override async Task<INoxCommandResult> ExecuteAsync(ExchangeCommand command)
        {
            // TODO: add DTO validation

            try
            {
                Nox.Store? store = await GetStore(command.StoreId);

                if (store == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Store cannot be found" };
                }

                var reservation = store.Reservations.FirstOrDefault(r => r.Id == command.ReservationId);

                if (reservation == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Reservation cannot be found" };
                }

                using var transaction = await DbContext.Database.BeginTransactionAsync();

                var destinatonAmount = store.Exchange(reservation);

                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // emit events
                await RaiseDomainEvents(reservation, destinatonAmount);

                _logger.LogInformation(message: $"Exchange for {reservation.Customer.Id} is successful");
            }
            catch (NoxDomainException noxEx)
            {
                _logger.LogError(noxEx, message: noxEx.Message);
                return new NoxCommandResult { IsSuccess = false, Message = noxEx.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "Cannot perform the exchange operation");
                return new NoxCommandResult { IsSuccess = false };
            }

            return new NoxCommandResult { IsSuccess = true };
        }

        private async Task<Nox.Store?> GetStore(int storeId)
        {
            return await DbContext
                            .Store
                            .Include(s => s.Reservations)
                            .ThenInclude(r => r.SourceCurrency)
                            .Include(s => s.Reservations)
                            .ThenInclude(r => r.DestinationCurrency)
                            .Include(s => s.Reservations)
                            .ThenInclude(r => r.Customer)
                            .Include(s => s.CashBalances)
                            .FirstOrDefaultAsync(s => s.Id == storeId);
        }

        private async Task RaiseDomainEvents(Reservation reservation, decimal destinatonAmount)
        {
            await SendBalanceChangedDomainEventAsync(
                new Nox.Events.BalanceChangedDomainEvent
                {
                    Payload = new BalanceChangedDto
                    {
                        StoreId = reservation.Store.Id,
                        Amount = reservation.SourceAmount,
                        CurrencyId = reservation.SourceCurrency.Id
                    }
                });

            await SendBalanceChangedDomainEventAsync(
                new Nox.Events.BalanceChangedDomainEvent
                {
                    Payload = new BalanceChangedDto
                    {
                        StoreId = reservation.Store.Id,
                        Amount = -destinatonAmount,
                        CurrencyId = reservation.DestinationCurrency.Id
                    }
                });
        }
    }
}
