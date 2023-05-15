using Microsoft.EntityFrameworkCore;
using Nox;
using Nox.Core.Interfaces.Entity.Commands;
using Nox.Core.Interfaces.Messaging;
using Nox.Commands;
using Nox.Dto;
using Nox.Core.Interfaces.Entity;

namespace Samples.Api.Domain.Store.Commands
{
    public class ExchangeCommandHandler : ExchangeCommandHandlerBase
    {
        public ExchangeCommandHandler(NoxDomainDbContext dbContext, INoxMessenger messenger)
            : base(dbContext, messenger)
        {
        }

        public override async Task<INoxCommandResult> ExecuteAsync(ExchangeCommand exchangeCommandDto)
        {
            // DTO validation

            using var transaction = await DbContext.Database.BeginTransactionAsync();

            try
            {
                var store = await DbContext
                                .Store
                                .Include(s => s.Reservations)
                                .ThenInclude(r => r.Customer)
                                .Include(s => s.CashBalances)
                                .FirstOrDefaultAsync(s => s.Id == exchangeCommandDto.StoreId);

                if (store == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Store cannot be found" };
                }

                var reservation = store.Reservations.FirstOrDefault(r => r.Id == exchangeCommandDto.ReservationId);

                if (reservation == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Reservation cannot be found" };
                }

                // Check balance - aggregate validation
                // TODO: change balances

                var destinatonAmount = reservation.SourceAmount * reservation.Rate;

                reservation.IsActive = false;

                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // emit events
                await RaiseDomainEvents(reservation, destinatonAmount);
            }
            catch (NoxDomainException noxEx)
            {
                return new NoxCommandResult { IsSuccess = false, Message = noxEx.Message };
            }
            catch (Exception)
            {
                // TODO: Handle failure and add logger
                return new NoxCommandResult { IsSuccess = false };
            }            

            return new NoxCommandResult { IsSuccess = true };
        }

        private async Task RaiseDomainEvents(Reservation exchangeCommandDto, decimal destinatonAmount)
        {
            await SendBalanceChangedDomainEventAsync(
                new Nox.Events.BalanceChangedDomainEvent
                {
                    Payload = new BalanceChangedDto
                    {
                        StoreId = exchangeCommandDto.Store.Id,
                        Amount = exchangeCommandDto.SourceAmount,
                        CurrencyId = exchangeCommandDto.SourceCurrency.Id                     
                    }
                });

            await SendBalanceChangedDomainEventAsync(
                new Nox.Events.BalanceChangedDomainEvent
                {
                    Payload = new BalanceChangedDto
                    {
                        StoreId = exchangeCommandDto.Store.Id,
                        Amount = -destinatonAmount,
                        CurrencyId = exchangeCommandDto.DestinationCurrency.Id
                    }
                });
        }
    }
}
