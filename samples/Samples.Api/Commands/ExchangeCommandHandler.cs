using Microsoft.EntityFrameworkCore;
using Nox;
using Nox.Core.Interfaces.Entity.Commands;
using Nox.Dto;

namespace Samples.Api.Commands
{
    public class ExchangeCommandHandler : Nox.Commands.ExchangeCommandHandlerBase
    {
        public ExchangeCommandHandler(NoxDbContext dbContext)
            : base(dbContext)
        {
        }

        public override async Task<INoxCommandResult> ExecuteAsync(ExchangeDto exchangeCommandDto)
        {
            // DTO validation

            // Check balance - aggregate validation

            var store = await DbContext
                .Set<Store>().AsQueryable()
                .FirstOrDefaultAsync(s => s.Id == exchangeCommandDto.StoreId);

            if (store == null)
            {
                return new NoxCommandResult { IsSuccess = false, Message = "Store cannot be found" };
            }

            // TODO: change balances

            await DbContext.SaveChangesAsync();

            // emit events


            return new NoxCommandResult { IsSuccess = true };
        }
    }
}
