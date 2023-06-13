using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Enumerations;
using Nox.Core.Interfaces.Messaging.Events;
using Nox.TestFixtures;
using NUnit.Framework;

namespace Nox.Messaging.Tests;

public class MessageExtensionTests: MessagingTestFixture 
{
    [Test]
    public void Can_Find_a_Create_Event_Implementation()
    {
        TestServices!.AddNoxEvents(Assembly.GetExecutingAssembly());
        BuildServiceProvider();
        
        var messages = TestServiceProvider!.GetRequiredService<IEnumerable<INoxEvent>>();
        var msg = messages.FindEventImplementation("Person", NoxEventType.Created);
        Assert.That(msg, Is.Not.Null);
    }

    [Test]
    public void Must_return_null_if_event_implementation_not_found()
    {
        TestServices!.AddNoxEvents(Assembly.GetExecutingAssembly());
        BuildServiceProvider();
        var messages = TestServiceProvider!.GetRequiredService<IEnumerable<INoxEvent>>();
        var msg = messages.FindEventImplementation("Person", NoxEventType.Deleted);
        Assert.That(msg, Is.Null);
    }
    
    [Test]
    public void Can_Map_Expando_Object_to_Message()
    {
        TestServices!.AddNoxEvents(Assembly.GetExecutingAssembly());
        BuildServiceProvider();
        var exObject = new ExpandoObject();
        exObject.AddProperty("Id", 1);
        exObject.AddProperty("Name", "Test User");
        exObject.AddProperty("Age", 50);
        var messages = TestServiceProvider!.GetRequiredService<IEnumerable<INoxEvent>>();
        var msg = messages.FindEventImplementation("Person", NoxEventType.Created);
        Assert.That(msg, Is.Not.Null);
        var instance = msg!.MapInstance(exObject, NoxEventSource.NoxDbContext);
        Assert.That(instance, Is.Not.Null);
        var createEvent = instance as PersonCreatedDomainEvent;
        Assert.That(createEvent!.Payload, Is.Not.Null);
        Assert.That(createEvent.Payload!.Id, Is.EqualTo(1));
        Assert.That(createEvent.Payload!.Name, Is.EqualTo("Test User"));
        Assert.That(createEvent.Payload!.Age, Is.EqualTo(50));
    }    
}