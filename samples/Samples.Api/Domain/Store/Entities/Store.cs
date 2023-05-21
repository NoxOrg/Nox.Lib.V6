using Nox.Core.Interfaces.Entity;

namespace Nox;

/// <summary>
/// Extends the generated Store class with rich domain logic operations.
/// </summary>
public partial class Store
{
    private const int DefaultActivePereiodInDays = 3;

    /// <summary>
    /// Adds a new reservation for a Customer.
    /// </summary>
    /// <param name="reservation">The reservation.</param>
    /// <exception cref="NoxDomainException">The exception in case validation failed.</exception>
    public void AddReservation(Reservation reservation)
    {
        // Domain Validation
        if (reservation.Customer.IsBlackListed)
        {
            throw new NoxDomainException("Reservation is not allowed for the customer.");
        }

        var destinationAmount = reservation.SourceAmount * reservation.Rate;

        var currentBalanceDestination = CashBalances
           .FirstOrDefault(b => b.Currency.Id == reservation.DestinationCurrency.Id)
           ?? throw new NoxDomainException($"Currency {reservation.DestinationCurrency.Name} is not supported.");

        if (currentBalanceDestination.Amount < destinationAmount)
        {
            throw new NoxDomainException($"Cannot perform the operation - insufficient amount of {reservation.DestinationCurrency.Name} is not supported.");
        }

        CheckLimits(currentBalanceDestination, destinationAmount);

        // Fill atributes according to the business rules
        reservation.IsActive = true;
        reservation.ExpirationTime = DateTime.Today.AddDays(DefaultActivePereiodInDays);

        Reservations.Add(reservation);
    }

    /// <summary>
    /// Performs an exchange operation for an existing reservation.
    /// </summary>
    /// <param name="reservation">The reservation.</param>
    /// <returns>The destination amount.</returns>
    /// <exception cref="NoxDomainException">The exception in case validation failed.</exception>
    public decimal Exchange(Reservation reservation)
    {
        if (!reservation.IsActive)
        {
            throw new NoxDomainException($"Reservation {reservation.Id} has been deactivated.");
        }

        var destinationAmount = reservation.SourceAmount * reservation.Rate;

        // Check balance - aggregate validation
        var currentBalanceDestination = CashBalances
            .FirstOrDefault(b => b.Currency.Id == reservation.DestinationCurrency.Id)
            ?? throw new NoxDomainException($"Currency {reservation.DestinationCurrency.Name} is not supported.");

        if (currentBalanceDestination.Amount < destinationAmount)
        {
            throw new NoxDomainException($"Cannot perform the operation - insufficient amount of {reservation.DestinationCurrency.Name} is not supported.");
        }

        CheckLimits(currentBalanceDestination, destinationAmount);

        var currentBalanceSource = CashBalances
            .FirstOrDefault(b => b.Currency.Id == reservation.DestinationCurrency.Id)
            ?? throw new NoxDomainException($"Currency {reservation.SourceCurrency.Name} is not supported.");

        // Change balances
        currentBalanceDestination.Amount += destinationAmount;
        currentBalanceSource.Amount -= reservation.SourceAmount;

        // Deactivate reservation
        reservation.IsActive = false;

        return destinationAmount;
    }

    private static void CheckLimits(CurrencyCashBalance currentBalanceDestination, decimal destinatonAmount)
    {
        if (currentBalanceDestination.OperationLimit.HasValue && currentBalanceDestination.OperationLimit < destinatonAmount)
        {
            throw new NoxDomainException($"Cannot perform the operation - operation limit is exceeded for destination currency.");
        }
    }
}
