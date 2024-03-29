//-----------------------------------------------------------------------------
// <copyright file="EntitySetWithKeyTemplateSegment.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Nox.Api.OData.Constants;

namespace Nox.Api.OData.Routing.TemplateSegments
{
    public class EntitySetWithKeyTemplateSegment : ODataSegmentTemplate
    {
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return $"({RoutingConstants.KeyParameterPathName})";
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

            string? entitySetName = entitysetNameObj as string;

            string? keyValue = keyObj as string;

            var edmEntitySet = context.Model.EntityContainer.FindEntitySet(entitySetName);

            if (edmEntitySet != null)
            {
                var entitySet = new EntitySetSegment(edmEntitySet);
                IEdmEntityType entityType = entitySet.EntitySet.EntityType();

                IEdmProperty keyProperty = entityType.Key().First();

                object newValue = ODataUriUtils.ConvertFromUriLiteral(keyValue, ODataVersion.V4, context.Model, keyProperty.Type);

                // for non FromODataUri, so update it, for example, remove the single quote for string value.
                context.UpdatedValues[RoutingConstants.KeyParameterName] = newValue;

                // For FromODataUri, let's refactor it later.
                string prefixName = ODataParameterValue.ParameterValuePrefix + RoutingConstants.KeyParameterName;
                context.UpdatedValues[prefixName] = new ODataParameterValue(newValue, keyProperty.Type);

                IDictionary<string, object> keysValues = new Dictionary<string, object>
                {
                    [keyProperty.Name] = newValue
                };

                var keySegment = new KeySegment(keysValues, entityType, entitySet.EntitySet);

                context.Segments.Add(keySegment);

                return true;
            }

            return false;
        }
    }
}
