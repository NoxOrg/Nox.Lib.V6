//-----------------------------------------------------------------------------
// <copyright file="HandleAllController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.OData.UriParser;
using Nox.Core.Interfaces;

namespace Nox.Api.OData.Controllers
{
    public class ODataController : Microsoft.AspNetCore.OData.Routing.Controllers.ODataController
    {

        private readonly IDynamicDbContext _context;

        public ODataController(IDynamicDbContext context)
        {
            _context = context;
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

        // post
        [EnableQuery]
        public IActionResult Post([FromBody]JsonElement data)
        {
            try
            {
                var entitySetName = GetEntitySetName(Request);

                var obj = _context.PostDynamicObject(entitySetName, data.GetRawText())!;

                return Created(obj);
            }
            catch (TargetInvocationException)
            {
                return new ConflictODataResult("Entity with the specified key already exists.");
            }

        }

    }
}
