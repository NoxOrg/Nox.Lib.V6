//-----------------------------------------------------------------------------
// <copyright file="EntitySetTemplateSegment.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.UriParser;
using Nox.Api.OData.Constants;

namespace Nox.Api.OData.Routing.TemplateSegments
{
    public class EntitySetTemplateSegment : ODataSegmentTemplate
    {
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return $"/{RoutingConstants.EntitySetParameterPathName}";
        }

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue(RoutingConstants.EntitySetParameterName, out object? classname))
            {
                return false;
            }

            var entitySetName = classname?.ToString();

            var edmEntitySet = context.Model.EntityContainer.FindEntitySet(entitySetName);

            //var edmEntitySet = context.Model.EntityContainer.FindEntitySet(entitySetName);
            if (edmEntitySet != null)
            {
                var segment = new EntitySetSegment(edmEntitySet);
                context.Segments.Add(segment);
                return true;
            }

            return false;
        }
    }
}
