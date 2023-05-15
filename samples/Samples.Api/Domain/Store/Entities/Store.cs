using Nox.Core.Interfaces.Entity;

namespace Nox;

public partial class Store
{
    public void AddReservation(Reservation reservation)
    {
        // Domain Validation
        if (reservation.Customer.IsBlackListed)
        {
            throw new NoxDomainException("Reservation is not allowed for the customer.");
        }

        Reservations.Add(reservation);
    }
}
