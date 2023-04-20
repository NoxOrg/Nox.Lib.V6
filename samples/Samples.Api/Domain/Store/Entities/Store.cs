namespace Nox;

public partial class Store
{
    public void AddReservation(Reservation reservation)
    {
        // TODO: Add Domain Validation
        
        Reservations.Add(reservation);
    }
}
