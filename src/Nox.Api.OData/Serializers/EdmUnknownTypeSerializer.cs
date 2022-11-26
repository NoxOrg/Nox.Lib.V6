using System.Text.Json;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Nox.Api.OData.Serializers.Helpers;

namespace Nox.Api.OData.Serializers
{
    public class EdmUnknownTypeSerializer : ODataEdmTypeSerializer
    {
        public EdmUnknownTypeSerializer(ODataPayloadKind payloadKind, IODataSerializerProvider serializerProvider) 
            : base(payloadKind, serializerProvider) { }

        public override ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
        {
            var value = CreateODataUntypedValue(graph, expectedType.AsPrimitive(), writeContext);
            if (value == null)
            {
                return new ODataNullValue();
            }
            return value;
        }

        public virtual ODataUntypedValue CreateODataUntypedValue(object graph, IEdmPrimitiveTypeReference primitiveType,
                ODataSerializerContext writeContext)
        {
            return CreateUntyped(graph, primitiveType, writeContext);
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataUntypedValue untyped, IEdmPrimitiveTypeReference primitiveType)
        {
            var typeName = primitiveType.FullName();

            untyped.TypeAnnotation = new ODataTypeAnnotation(typeName);
        }

        internal static ODataUntypedValue CreateUntyped(object value, IEdmPrimitiveTypeReference primitiveType,
            ODataSerializerContext writeContext)
        {

            var untypedValue = ConvertUntypedValue(value, primitiveType, writeContext?.TimeZone);

            var serializedValue = JsonSerializer.Serialize(untypedValue);

            var untyped = new ODataUntypedValue()
            {
                RawValue = $" {serializedValue}"
            };

            if (writeContext != null)
            {
                AddTypeNameAnnotationAsNeeded(untyped, primitiveType);
            }

            return untyped;
        }

        internal static object? ConvertUntypedValue(object value, IEdmPrimitiveTypeReference primitiveType, TimeZoneInfo? timeZoneInfo)
        {
            if (value == null || timeZoneInfo == null)
            {
                return null;
            }

            Type type = value.GetType();
            if (primitiveType != null && primitiveType.IsDate() && TypeHelper.IsDateTime(type))
            {
                Date dt = (DateTime)value;
                return dt;
            }

            if (primitiveType != null && primitiveType.IsTimeOfDay() && TypeHelper.IsTimeSpan(type))
            {
                TimeOfDay tod = (TimeSpan)value;
                return tod;
            }

            // Since ODL doesn't support "DateOnly", we have to use Date defined in ODL.
            if (primitiveType != null && primitiveType.IsDate() && TypeHelper.IsDateOnly(type))
            {
                DateOnly dateOnly = (DateOnly)value;
                return new Date(dateOnly.Year, dateOnly.Month, dateOnly.Day);
            }

            // Since ODL doesn't support "TimeOnly", we have to use TimeOfDay defined in ODL.
            if (primitiveType != null && primitiveType.IsTimeOfDay() && TypeHelper.IsTimeOnly(type))
            {
                TimeOnly timeOnly = (TimeOnly)value;
                return new TimeOfDay(timeOnly.Hour, timeOnly.Minute, timeOnly.Second, timeOnly.Millisecond);
            }

            return ConvertUnsupportedPrimitives(value, timeZoneInfo);
        }

        internal static object? ConvertUnsupportedPrimitives(object value, TimeZoneInfo timeZoneInfo)
        {
            if (value != null)
            {
                Type type = value.GetType();

                // Note that type cannot be a nullable type as value is not null and it is boxed.
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                        return new string((char)value, 1);

                    case TypeCode.UInt16:
                        return (int)(ushort)value;

                    case TypeCode.UInt32:
                        return (long)(uint)value;

                    case TypeCode.UInt64:
                        return checked((long)(ulong)value);

                    case TypeCode.DateTime:
                        DateTime dateTime = (DateTime)value;
                        return TimeZoneInfoHelper.ConvertToDateTimeOffset(dateTime, timeZoneInfo);

                    default:
                        if (type == typeof(char[]))
                        {
                            return new string(value as char[]);
                        }
                        break;
                }
            }

            return value;
        }



    }
}
