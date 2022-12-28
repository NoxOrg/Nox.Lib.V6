using System.Collections.Immutable;
using System.Dynamic;
using Nox.Core.Enumerations;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Messaging.Events;

namespace Nox.Messaging;

public static class MessageExtensions
{
    public static INoxEvent? FindEventImplementation(this IEnumerable<INoxEvent> messages, string entityName, NoxEventType eventType)
    {
        foreach (var msg in messages.ToImmutableList())
        {
            var type = msg.GetType();
            var baseType = type.BaseType;
            if (baseType == null) return null;
            if (baseType.GenericTypeArguments.Any(gta => gta.Name == entityName))
            {

                switch (eventType)
                {
                    case NoxEventType.Created:
                        if (baseType.Name.StartsWith(nameof(NoxCreatedEvent<IDynamicEntity>))) return msg;
                        break;
                    case NoxEventType.Deleted:
                        if (baseType.Name.StartsWith(nameof(NoxDeletedEvent<IDynamicEntity>))) return msg;
                        break;
                    case NoxEventType.Updated:
                        if (baseType.Name.StartsWith(nameof(NoxUpdatedEvent<IDynamicEntity>))) return msg;
                        break;
                }
            }
        }
        return null;
    }

    public static object MapInstance(this INoxEvent template, Object source, NoxEventSource eventSource)
    {
        var result = Activator.CreateInstance(template.GetType())!;
        var props = template.GetType().GetProperties();
        var eventSourceProp = props.FirstOrDefault(p => p.Name.ToLower() == "eventsource");
        if (eventSourceProp != null)
        {
            eventSourceProp.SetValue(result, eventSource);
        }
        var payloadProp = template.GetType().GetProperties().FirstOrDefault(p => p.Name.ToLower() == "payload");
        if (payloadProp != null)
        {
            var payload = Activator.CreateInstance(payloadProp.PropertyType);
            var sourceDict = source as IDictionary<string, object?>;
            if (payload != null)
            {
                foreach (var prop in payload.GetType().GetProperties())
                {
                    var sourceVal = sourceDict?[prop.Name];
                    if (sourceVal == null) continue;

                    try
                    {
                        // when reading from json, numbers and booleans are long - will AutoMapper handle this better? A.S.
                        if(sourceVal.GetType().Equals(prop.PropertyType))
                        {
                            prop.SetValue(payload, sourceVal);
                        }
                        else if (sourceVal is long) 
                        {
                            if (prop.PropertyType.Equals(typeof(Int32)))
                            {
                                prop.SetValue(payload, Convert.ToInt32(sourceVal));
                            }
                            else if (prop.PropertyType.Equals(typeof(bool)))
                            {
                                prop.SetValue(payload, Convert.ToBoolean(sourceVal));
                            }
                            else 
                            {
                                prop.SetValue(payload, Convert.ChangeType(sourceVal, prop.PropertyType));
                            }
                        }
                        else if (sourceVal is int && prop.PropertyType.Equals(typeof(bool)))
                        {
                            prop.SetValue(payload, Convert.ToBoolean(sourceVal));
                        }
                        else
                        {
                            prop.SetValue(payload, Convert.ChangeType(sourceVal, prop.PropertyType));
                        }
                    }
                    catch 
                    {
                        throw new InvalidCastException(string.Format(ExceptionResources.MapInstanceCastException, template.GetType().Name, payload.GetType().Name, prop.Name, sourceVal.GetType().Name,
                            prop.PropertyType.Name, payload.GetType().Name));
                    }
                }    
            }
            payloadProp.SetValue(result, payload);
        }
        return result;
    }
}