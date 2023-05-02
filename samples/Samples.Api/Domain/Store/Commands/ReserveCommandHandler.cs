using Microsoft.EntityFrameworkCore;
using Nox;
using Nox.Core.Interfaces.Entity.Commands;
using Nox.Core.Interfaces.Messaging;
using Nox.Commands;
using Nox.Dto;

namespace Samples.Api.Domain.Store.Commands
{
    public class ReserveCommandHandler : ReserveCommandHandlerBase
    {
        public ReserveCommandHandler(NoxDomainDbContext dbContext, INoxMessenger messenger)
            : base(dbContext, messenger)
        {
        }

        public override async Task<INoxCommandResult> ExecuteAsync(ReserveCommand reserveCommandDto)
        {
            // DTO validation           

            using var transaction = await DbContext.Database.BeginTransactionAsync();

            try
            {
                var store = await DbContext
                    .Store
                    .Include(s => s.Reservations)
                    .FirstOrDefaultAsync(s => s.Id == reserveCommandDto.StoreId);

                if (store == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Store cannot be found" };
                }

                var customer = await DbContext
                    .Customer
                    .FirstOrDefaultAsync(s => s.Id == reserveCommandDto.CustomerId);

                if (customer == null)
                {
                    return new NoxCommandResult { IsSuccess = false, Message = "Customer cannot be found" };
                }

                // Add reservation
                store.AddReservation(
                    new Reservation
                    {
                        IsActive = true,
                        SourceAmount = reserveCommandDto.SourceAmount,
                        Customer = customer
                    });

                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // TODO: Handle failure
                return new NoxCommandResult { IsSuccess = false };
            }

            // emit events
            await SendBalanceChangedDomainEventAsync(
                new Nox.Events.BalanceChangedDomainEvent
                {
                    Payload = new BalanceChangedDto
                    {
                        StoreId = reserveCommandDto.StoreId,
                        Amount = reserveCommandDto.SourceAmount,
                        CurrencyId = reserveCommandDto.SourceCurrencyId
                    }
                });

            return new NoxCommandResult { IsSuccess = true };
        }
    }
}
