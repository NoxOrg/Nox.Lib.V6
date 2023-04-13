using Microsoft.EntityFrameworkCore;
using Nox;
using Nox.Dto;

namespace Samples.Api.Domain.Store.Queries
{
    public class ActiveReservations : Nox.Queries.ActiveReservationsQuery
    {
        public ActiveReservations(NoxDomainDbContext dbContext) : base(dbContext)
        {
        }

        public async override Task<IList<ReservationInfoDto>> ExecuteAsync(int storeId)
        {
            var store = await DbContext
                .Store
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(s => s.Id == storeId) ?? throw new Exception("Store cannot be found");

            return store.Reservations
                .Where(r => r.IsActive)
                .Select(r => new ReservationInfoDto
                {
                    SourceCurrencyId = r.SourceCurrency.Id,
                    DestinationCurrencyId = r.DestinationCurrency.Id,
                    ExpirationTime = r.ExpirationTime,
                    Rate = r.Rate,
                    SourceAmount = r.SourceAmount,
                })
                .ToList();
        }
    }
}
