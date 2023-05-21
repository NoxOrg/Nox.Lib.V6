using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nox.Api.OData.Constants;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Entity;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IO;

namespace Nox.Api.OData.Swagger
{
    /// <summary>
    /// This filter is intended to format default OData generated API
    /// to more usable form specyfing particular entities available
    /// </summary>
    public class ODataEntitySectionsSwaggerFilter : IDocumentFilter
    {
        private static IDynamicService? _dynamicService;
        private static ILogger? _logger;

        private static readonly IReadOnlySet<string> _parametersToRemoveFromSwagger = new HashSet<string>
        {
            RoutingConstants.EntitySetParameterName,
            RoutingConstants.PropertyParameterName,
            RoutingConstants.NavigationParameterName
        };

        /// <summary>
        /// Interface method that applies filter change to swagger definition
        /// </summary>
        /// <param name="swaggerDoc">Swagger definition</param>
        /// <param name="context">Filterting context</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (_dynamicService?.Entities == null ||
                !_dynamicService.Entities.Any())
            {
                _logger?.LogWarning("Warning! Nox configuration or entities were not found in ODataCustomSwaggerFilter. Using default API definitions.");
                return;
            }

            var ownedEntities = _dynamicService
                .Entities
                .SelectMany(e => e.Value.OwnedRelationships)
                .Select(r => r.Entity)
                .Distinct();

            var entities = _dynamicService!.Entities!
                .Where(x => !string.IsNullOrWhiteSpace(x.Value.PluralName)
                    && !ownedEntities.Contains(x.Key)) // show only independent entities
                .Select(x => x.Value);

            var odataPathItems = swaggerDoc.Paths
                .Where(p =>
                    p.Key.Contains($"/{RoutingConstants.ODATA_ROUTE_PREFIX}/{RoutingConstants.EntitySetParameterPathName}", StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            // Loop to convert generic odata path items to per entity ones
            foreach (var entity in entities.OrderBy(e => e.PluralName))
            {
                foreach (var odataPathItem in odataPathItems.Union(swaggerDoc.Paths.Where(path =>
                    path.Key.Contains($"/{RoutingConstants.ODATA_ROUTE_PREFIX}/{entity.PluralName}/", StringComparison.OrdinalIgnoreCase)).Distinct().ToList()))
                {
                    // Hide property endpoint as it not required at the moment
                    if (odataPathItem.Key.Contains(RoutingConstants.PropertyParameterPathName))
                    {
                        continue;
                    }

                    OpenApiPathItem newPathItem = CreateCopyWithOperations(entity.Name, odataPathItem.Value);
                    swaggerDoc.Paths.Remove(odataPathItem.Key);

                    if (odataPathItem.Key.Contains(RoutingConstants.NavigationParameterPathName))
                    {
                        AddNewPathItemPerEachParentEntity(swaggerDoc, entity, odataPathItem.Key, newPathItem);
                    }
                    else
                    {
                        AddNewPathItemToItemsList(swaggerDoc, entity, odataPathItem.Key, newPathItem);
                    }
                }
            }

            // Remove generic odata path items
            foreach (var path in odataPathItems)
            {
                swaggerDoc.Paths.Remove(path.Key);
            }
        }

        private static OpenApiPathItem CreateCopyWithOperations(string entityName, OpenApiPathItem odataPathItem)
        {
            var newPathItem = CreateACopyOfExistingPathItem(odataPathItem);

            // Data grouped by api path, so each operation (GET, POST, etc.) should be 
            // handled individually
            foreach (var operation in odataPathItem.Operations
                .Where(o => o.Key != OperationType.Patch && o.Key != OperationType.Put)) // Exclude Patch/Put
            {
                var newOperation = CreateACopyOfExistingOperation(operation);

                AddOldOperationParametersThatAreNotReplacedToNewOperation(operation, newOperation);

                // Split into sections
                AddSectionTag(newOperation, entityName);

                newPathItem.AddOperation(operation.Key, newOperation);
            }

            return newPathItem;
        }

        /// <summary>
        /// Dependency injection is not available for filters, so this needs to be set manually.
        /// A non-empty list of entities to be used in generation is expected
        /// No change will be applied if empty
        /// </summary>
        /// <param name="projectConfiguration">Project configuration containing entities list</param>
        /// <param name="logger">Logger to be used in output</param>
        public static void Initialize(
            ILogger<ODataEntitySectionsSwaggerFilter>? logger,
            IDynamicService? dynamicService)
        {
            _logger = logger;
            _dynamicService = dynamicService;
        }

        private static void AddOldOperationParametersThatAreNotReplacedToNewOperation(
            KeyValuePair<OperationType, OpenApiOperation> operation,
            OpenApiOperation newOperation)
        {
            foreach (var existingParameter in operation.Value.Parameters)
            {
                if (!_parametersToRemoveFromSwagger.Contains(existingParameter.Name))
                {
                    newOperation.Parameters.Add(existingParameter);
                }
            }
        }

        private static void AddSectionTag(
            OpenApiOperation newOperation,
            string sectionName)
        {
            newOperation.Tags = new[]
            {
                new OpenApiTag
                {
                    Name = sectionName,
                }
            };
        }

        private static void AddNewPathItemPerEachParentEntity(
            OpenApiDocument swaggerDoc,
            IEntity entity,
            string key,
            OpenApiPathItem newPathItem)
        {
            if (entity?.RelatedParents == null ||
                !entity.RelatedParents.Any())
            {
                return;
            }

            foreach (var parentEntity in entity!.RelatedParents!)
            {
                swaggerDoc.Paths.Add(
                    ReplaceODataEntityName(key, entity.PluralName)
                        .Replace(RoutingConstants.NavigationParameterPathName, parentEntity, StringComparison.OrdinalIgnoreCase),
                    newPathItem);
            }
        }

        private static void AddNewPathItemToItemsList(
            OpenApiDocument swaggerDoc,
            IEntity entity,
            string key,
            OpenApiPathItem newPathItem)
        {
            swaggerDoc.Paths.Add(
                ReplaceODataEntityName(key, entity.PluralName),
                newPathItem);
        }

        private static string ReplaceODataEntityName(string currentPath, string pluralEntityName)
        {
            return currentPath.Replace($"/{RoutingConstants.ODATA_ROUTE_PREFIX}/{RoutingConstants.EntitySetParameterPathName}", $"/{RoutingConstants.ODATA_ROUTE_PREFIX}/{pluralEntityName}", StringComparison.OrdinalIgnoreCase);
        }

        private static OpenApiPathItem CreateACopyOfExistingPathItem(OpenApiPathItem odataPathItem)
        {
            return new OpenApiPathItem
            {
                Servers = odataPathItem.Servers,
                Parameters = odataPathItem.Parameters,
                Extensions = odataPathItem.Extensions,
                Summary = odataPathItem.Summary,
                Description = odataPathItem.Description
            };
        }

        private static OpenApiOperation CreateACopyOfExistingOperation(KeyValuePair<OperationType, OpenApiOperation> operation)
        {
            return new OpenApiOperation
            {
                Summary = operation.Value.Summary,
                Description = operation.Value.Description,
                Responses = operation.Value.Responses,
                Callbacks = operation.Value.Callbacks,
                Deprecated = operation.Value.Deprecated,
                Extensions = operation.Value.Extensions,
                ExternalDocs = operation.Value.ExternalDocs,
                RequestBody = operation.Value.RequestBody,
                Security = operation.Value.Security,
                Servers = operation.Value.Servers,
                Tags = operation.Value.Tags
            };
        }
    }
}
