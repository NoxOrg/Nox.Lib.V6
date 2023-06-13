//-----------------------------------------------------------------------------
// <copyright file="StaticNameSegment.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Nox.Api.OData.Constants;

namespace Nox.Api.OData.Routing.TemplateSegments
{
    public class PropertyTemplateSegment : ODataSegmentTemplate
    {
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return $"/{RoutingConstants.PropertyParameterPathName}";
        }

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue(RoutingConstants.EntitySetParameterName, out object? entitysetNameObj))
            {
                return false;
            }

            if (!context.RouteValues.TryGetValue(RoutingConstants.KeyParameterName, out object? keyObj))
            {
                return false;
            }

            if (!context.RouteValues.TryGetValue(RoutingConstants.PropertyParameterName, out object? propertyObj))
            {
                return false;
            }

            string? entitySetName = entitysetNameObj as string;

            string? keyValue = keyObj as string;

            string? propertyName = propertyObj as string;

            var edmEntitySet = context.Model.EntityContainer.FindEntitySet(entitySetName);

            if (edmEntitySet != null)
            {
                KeySegment? keySegment = context.Segments.Last() as KeySegment;
                IEdmEntityType? entityType = keySegment?.EdmType as IEdmEntityType;
                IEdmProperty? edmProperty = entityType?.Properties()
                    .FirstOrDefault(p => p.PropertyKind == EdmPropertyKind.Structural && p.Name.Equals(propertyName));
                if (edmProperty != null)
                {
                    var seg = new PropertySegment(edmProperty as IEdmStructuralProperty);
                    context.Segments.Add(seg);
                    return true;
                }
            }


            return false;
        }
    }
}
