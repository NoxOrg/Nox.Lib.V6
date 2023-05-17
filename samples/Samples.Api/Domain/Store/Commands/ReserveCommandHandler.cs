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
    /// Performs a reservation operation for an existing customer.
    /// Handler is derived from the automatically generated.
    /// </summary>
    public class ReserveCommandHandler : ReserveCommandHandlerBase
    {
        private readonly ILogger _logger;

        public ReserveCommandHandler(NoxDomainDbContext dbContext, INoxMessenger messenger, ILogger<ReserveCommandHandler> logger)
            : base(dbContext, messenger)
        {
            _logger = logger;
        }

        public override async Task<INoxCommandResult> ExecuteAsync(ReserveCommand command)
        {
            // TODO: add DTO validation

            try
            {
                var store = await DbContext
                    .Store
                    .Include(s => s.Reservations)
                    .FirstOrDefaultAsync(s => s.Id == command.StoreId);

                if (store == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Store cannot be found" };
                }

                var customer = await DbContext.Customer.FirstOrDefaultAsync(s => s.Id == command.CustomerId);

                if (customer == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Customer cannot be found" };
                }

                var destinationCurrency = await DbContext.Currency.FirstOrDefaultAsync(c => c.Id == command.DestinationCurrencyId);

                if (destinationCurrency == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Destination Currency cannot be found" };
                }

                var sourceCurrency = await DbContext.Currency.FirstOrDefaultAsync(c => c.Id == command.SourceCurrencyId);

                if (sourceCurrency == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Source Currency cannot be found" };
                }

                using var transaction = await DbContext.Database.BeginTransactionAsync();

                // Add reservation
                store.AddReservation(
                    new Reservation
                    {
                        SourceAmount = command.SourceAmount,
                        Customer = customer,
                        DestinationCurrency = destinationCurrency,
                        SourceCurrency = sourceCurrency,
                        Rate = command.Rate,
                    });

                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(message: $"Reservation for {customer.Id} is successful");
            }
            catch (NoxDomainException noxEx)
            {
                _logger.LogError(noxEx, message: noxEx.Message);
                return new NoxCommandResult { IsSuccess = false, Message = noxEx.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "Cannot perform the reservation operation");
                return new NoxCommandResult { IsSuccess = false };
            }

            // emit events
            await SendBalanceChangedDomainEventAsync(
                new Nox.Events.BalanceChangedDomainEvent
                {
                    Payload = new BalanceChangedDto
                    {
                        StoreId = command.StoreId,
                        Amount = command.SourceAmount,
                        CurrencyId = command.SourceCurrencyId
                    }
                });

            return new NoxCommandResult { IsSuccess = true };
        }
    }
}
