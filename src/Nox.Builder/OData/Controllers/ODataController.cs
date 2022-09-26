//-----------------------------------------------------------------------------
// <copyright file="HandleAllController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.UriParser;
using Nox.OData.Models;

namespace Nox.OData.Controllers
{
    public class ODataController : Microsoft.AspNetCore.OData.Routing.Controllers.ODataController
    {

        private readonly DynamicDbContext _context;

        public ODataController(DynamicDbContext context)
        {
            _context = context;

            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        private static string GetEntitySetName(HttpRequest request) =>
            ((EntitySetSegment)request.ODataFeature().Path.FirstSegment).EntitySet.Name;

        // Get entityset
        // odata/{entityset}
        [EnableQuery]
        public IActionResult Get()
        {
            var entitySetName = GetEntitySetName(Request);

            var collection = _context.GetDynamicCollection(entitySetName);

            return Ok(collection);
        }

        // Get entityset(key)
        // /odata/{entityset}({key})
        [EnableQuery]
        public IActionResult Get(string key)
        {
            var entitySetName = GetEntitySetName(Request);

            var singleResult = _context.GetDynamicSingleResult(entitySetName, key)!;

            return Ok(singleResult);
        }

        // info/odata/{entityset}({key})/{property}
        public IActionResult GetProperty(string key, string property)
        {
            var entitySetName = GetEntitySetName(Request);

            var propValue = _context.GetDynamicObjectProperty(entitySetName, key, property)!;

            return Ok(propValue);
        }

        // info/odata/{entityset}({key})/{navigation}
        public IActionResult GetNavigation(string key, string navigation)
        {
            var entitySetName = GetEntitySetName(Request);

            var singleResult = _context.GetDynamicNavigation(entitySetName, key, navigation)!;

            return Ok(singleResult);
        }

    }
}
