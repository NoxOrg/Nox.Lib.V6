using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Nox.Api.OData.Serializers
{
    public class CustomODataSerializerProvider : ODataSerializerProvider
    {

        private readonly IODataEdmTypeSerializer _serializer;

        public CustomODataSerializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {
            _serializer = new EdmUnknownTypeSerializer(ODataPayloadKind.Value, this);
        }

        public override IODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType.Definition.TypeKind == EdmTypeKind.Untyped)
                return _serializer;
            else
                return base.GetEdmTypeSerializer(edmType);
        }
    }
}
