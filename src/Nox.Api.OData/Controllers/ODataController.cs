//-----------------------------------------------------------------------------
// <copyright file="HandleAllController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.OData.UriParser;
using Nox.Core.Interfaces.Database;
using Swashbuckle.AspNetCore.Annotations;

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
        [SwaggerOperation(OperationId = "Get_Nox_Dynamic_Entities")]
        public IActionResult Get(string key)
        {
            var entitySetName = GetEntitySetName(Request);

            var singleResult = _context.GetDynamicSingleResult(entitySetName, key)!;

            return Ok(singleResult);
        }

        // info/odata/{entityset}({key})/{property}
        [SwaggerOperation(OperationId = "Get_Nox_Dynamic_Property")]
        public IActionResult GetProperty(string key, string property)
        {
            var entitySetName = GetEntitySetName(Request);

            var propValue = _context.GetDynamicObjectProperty(entitySetName, key, property)!;

            return Ok(propValue);
        }

        // info/odata/{entityset}({key})/{navigation}
        [SwaggerOperation(OperationId = "Get_Nox_Dynamic_Navigation")]
        public IActionResult GetNavigation(string key, string navigation)
        {
            var entitySetName = GetEntitySetName(Request);

            var singleResult = _context.GetDynamicNavigation(entitySetName, key, navigation)!;

            return Ok(singleResult);
        }

        // post
        [EnableQuery]
        [SwaggerOperation(OperationId = "Post_Nox_Dynamic_Entity")]
        public IActionResult Post([FromBody]JsonElement data)
        {
            try
            {
                var entitySetName = GetEntitySetName(Request);

                var obj = _context.PostDynamicObject(entitySetName, data.GetRawText())!;

                return Created(obj);
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                return new BadRequestODataResult(ex.Message);
            }

        }
        
        // put
        // /odata/{entityset}
        [EnableQuery]
        [SwaggerOperation(OperationId = "Put_Nox_Dynamic_Entity")]
        public IActionResult Put([FromBody]JsonElement data)
        {
            try
            {
                var entitySetName = GetEntitySetName(Request);

                var obj = _context.PutDynamicObject(entitySetName, data.GetRawText())!;
                return Updated(obj);
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                return new BadRequestODataResult(ex.Message);
            }

        }
        
        // patch
        // /odata/{entityset}({key})
        [EnableQuery]
        [SwaggerOperation(OperationId = "Patch_Nox_Dynamic_Entity")]
        public IActionResult Patch(string key, [FromBody]JsonElement data)
        {
            try
            {
                var entitySetName = GetEntitySetName(Request);

                var obj = _context.PatchDynamicObject(entitySetName, key, data.GetRawText())!;
                return Updated(obj);
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                return new BadRequestODataResult(ex.Message);
            }

        }
        
        // delete
        // /odata/{entityset}({key})
        [EnableQuery]
        [SwaggerOperation(OperationId = "Delete_Nox_Dynamic_Entity")]
        public IActionResult Delete(string key)
        {
            try
            {
                var entitySetName = GetEntitySetName(Request);

                _context.DeleteDynamicObject(entitySetName, key);
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                return new BadRequestODataResult(ex.Message);
            }

        }

    }
}
