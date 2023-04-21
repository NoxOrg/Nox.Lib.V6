using Microsoft.EntityFrameworkCore;
using Nox;
using Nox.Core.Interfaces.Entity.Commands;
using Nox.Core.Interfaces.Messaging;
using Nox.Commands;
using Nox.Dto;

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
                                .Include(s => s.CacheBalances)
                                .FirstOrDefaultAsync(s => s.Id == exchangeCommandDto.StoreId);

                if (store == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Store cannot be found" };
                }

                // Check balance - aggregate validation

                // TODO: change balances

                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // TODO: Handle failure and add logger
                return new NoxCommandResult { IsSuccess = false };
            }

            // emit events
            await RaiseDomainEvents(exchangeCommandDto);

            return new NoxCommandResult { IsSuccess = true };
        }

        private async Task RaiseDomainEvents(ExchangeCommand exchangeCommandDto)
        {
            await SendBalanceChangedDomainEventAsync(
                new Nox.Events.BalanceChangedDomainEvent
                {
                    Payload = new BalanceChangedDto
                    {
                        StoreId = exchangeCommandDto.StoreId,
                        Amount = exchangeCommandDto.SourceAmount,
                        CurrencyId = exchangeCommandDto.SourceCurrencyId
                    }
                });

            await SendBalanceChangedDomainEventAsync(
                new Nox.Events.BalanceChangedDomainEvent
                {
                    Payload = new BalanceChangedDto
                    {
                        StoreId = exchangeCommandDto.StoreId,
                        Amount = -exchangeCommandDto.DestinationAmount,
                        CurrencyId = exchangeCommandDto.DestinationCurrencyId
                    }
                });
        }
    }
}
