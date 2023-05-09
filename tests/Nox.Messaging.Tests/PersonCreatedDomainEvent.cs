using Nox.Core.Interfaces.Messaging.Events;
using Nox.TestFixtures.Entities;

namespace Nox.Messaging.Tests;

public class PersonCreatedDomainEvent: NoxCreatedEvent<Person>
{
}