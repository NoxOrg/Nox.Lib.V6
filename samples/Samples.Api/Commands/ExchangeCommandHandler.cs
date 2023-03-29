using Microsoft.EntityFrameworkCore;
using Nox;
using Nox.Core.Interfaces.Entity.Commands;
using Nox.Core.Interfaces.Messaging;
using Nox.Dto;

namespace Samples.Api.Commands
{
    public class ExchangeCommandHandler : Nox.Commands.ExchangeCommandHandlerBase
    {
        public ExchangeCommandHandler(NoxDbContext dbContext, INoxMessenger messenger)
            : base(dbContext, messenger)
        {
        }

        public override async Task<INoxCommandResult> ExecuteAsync(ExchangeDto exchangeCommandDto)
        {
            // DTO validation

            // Check balance - aggregate validation

            var store = await DbContext
                .Store
                .FirstOrDefaultAsync(s => s.Id == exchangeCommandDto.StoreId);

            if (store == null)
            {
                return new NoxCommandResult { IsSuccess = false, Message = "Store cannot be found" };
            }

            // TODO: change balances

            await DbContext.SaveChangesAsync();

            // emit events
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

            return new NoxCommandResult { IsSuccess = true };
        }
    }
}
