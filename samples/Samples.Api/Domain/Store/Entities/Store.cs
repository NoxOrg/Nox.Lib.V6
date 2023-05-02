using MassTransit.RabbitMqTransport.Topology;

namespace Nox;

public partial class Store
{
    public void AddReservation(Reservation reservation)
    {
        // Domain Validation
        if (reservation.Customer.IsBlackListed)
        {

        }

        Reservations.Add(reservation);
    }
}
