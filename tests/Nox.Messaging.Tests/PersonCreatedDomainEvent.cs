using Nox.Messaging.Events;
using Nox.TestFixtures.Entities;

namespace Nox.Messaging.Tests;

public class PersonCreatedDomainEvent: NoxCreateEvent<Person>
{
}