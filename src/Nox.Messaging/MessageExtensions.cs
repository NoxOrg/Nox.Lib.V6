using System.Collections.Immutable;
using System.Dynamic;
using Nox.Core.Interfaces;
using Nox.Messaging.Enumerations;
using Nox.Messaging.Events;

namespace Nox.Messaging;

public static class MessageExtensions
{
    public static INoxEvent? FindEventImplementation(this IEnumerable<INoxEvent> messages, IEntity entity, NoxEventTypeEnum eventType)
    {
        foreach (var msg in messages.ToImmutableList())
        {
            var type = msg.GetType();
            var baseType = type.BaseType;
            if (baseType == null) return null;
            if (baseType.GenericTypeArguments.Any(gta => gta.Name == entity.Name))
            {

                switch (eventType)
                {
                    case NoxEventTypeEnum.Create:
                        if (baseType.Name == "NoxCreateEvent`1") return msg;
                        break;
                    case NoxEventTypeEnum.Delete:
                        if (baseType.Name == "NoxDeleteEvent`1") return msg;
                        break;
                    case NoxEventTypeEnum.Update:
                        if (baseType.Name == "NoxUpdateEvent`1") return msg;
                        break;
                }
            }
        }
        return null;
    }
    
    

    public static object MapInstance(this INoxEvent template, ExpandoObject source)
    {
        var result = Activator.CreateInstance(template.GetType())!;
        var payloadProp = template.GetType().GetProperties().FirstOrDefault(p => p.Name.ToLower() == "payload");
        if (payloadProp != null)
        {
            var payload = Activator.CreateInstance(payloadProp.PropertyType);
            var sourceDict = source as IDictionary<string, object>;
            if (payload != null)
            {
                foreach (var prop in payload.GetType().GetProperties())
                {
                    var sourceVal = sourceDict[prop.Name];
                    prop.SetValue(payload, sourceVal);
                }    
            }
            payloadProp.SetValue(result, payload);
        }
        return result;
    }
}