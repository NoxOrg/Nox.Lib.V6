using Microsoft.OpenApi.Models;
using Nox.Api.OData.Constants;
using Nox.Core.Configuration;
using Nox.Core.Interfaces.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nox.Api.OData.Swagger
{
    public class ODataCustomSwaggerFilter : IDocumentFilter
    {
        private static IProjectConfiguration? _projectConfiguration;

        private static IReadOnlySet<string> _parametersToRemoveFromSwagger = new HashSet<string>
        {
            RoutingConstants.EntitySetParameterName,
            RoutingConstants.PropertyParameterName,
            RoutingConstants.NavigationParameterName
        };

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (_projectConfiguration?.Entities == null ||
                !_projectConfiguration.Entities.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Warning! Nox configuration or entities were not found in ODataCustomSwaggerFilter. Using default API definitions.");
                Console.ResetColor();
                return;
            }

            var entities = _projectConfiguration!.Entities!
                .Where(x => !string.IsNullOrWhiteSpace(x.PluralName));

            var odataPathItems = swaggerDoc.Paths
                .Where(p =>
                    p.Key.Contains($"/{RoutingConstants.ODATA_ROUTE_PREFIX}/{RoutingConstants.EntitySetParameterPathName}", StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            // Loop to convert generic odata path items to per entity ones
            foreach (var entity in entities)
            {
                foreach (var odataPathItem in odataPathItems)
                {
                    // Hide property endpoint as it not required at the moment
                    if (odataPathItem.Key.Contains(RoutingConstants.PropertyParameterPathName))
                    {
                        continue;
                    }

                    var newPathItem = CreateACopyOfExistingPathItem(odataPathItem);

                    // Data grouped by api path, so each operation (GET, POST, etc.) should be 
                    // handled individually
                    foreach (var operation in odataPathItem.Value.Operations)
                    {
                        var newOperation = CreateACopyOfExistingOperation(operation);

                        AddOldOperationParametersThatAreNotReplacedToNewOperation(operation, newOperation);

                        // Split into sections
                        AddSectionTag(newOperation, entity.Name);

                        newPathItem.AddOperation(operation.Key, newOperation);
                    }

                    if (odataPathItem.Key.Contains(RoutingConstants.NavigationParameterPathName))
                    {
                        AddNewPathItemPerEachParentEntity(swaggerDoc, entity, odataPathItem, newPathItem);
                    }
                    else
                    {
                        AddNewPathItemToItemsList(swaggerDoc, entity, odataPathItem, newPathItem);
                    }
                }
            }

            // Remove generic odata path items
            foreach (var path in odataPathItems)
            {
                swaggerDoc.Paths.Remove(path.Key);
            }
        }

        public static void SetProjectConfiguration(IProjectConfiguration? projectConfiguration)
        {
            _projectConfiguration = projectConfiguration;
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

        private void AddSectionTag(
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

        private void AddNewPathItemPerEachParentEntity(
            OpenApiDocument swaggerDoc,
            EntityConfiguration entity,
            KeyValuePair<string, OpenApiPathItem> odataPathItem,
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
                    ReplaceODataEntityName(odataPathItem.Key, entity.PluralName)
                        .Replace(RoutingConstants.NavigationParameterPathName, parentEntity, StringComparison.OrdinalIgnoreCase),
                    newPathItem);
            }
        }

        private static void AddNewPathItemToItemsList(
            OpenApiDocument swaggerDoc,
            EntityConfiguration entity,
            KeyValuePair<string, OpenApiPathItem> odataPathItem,
            OpenApiPathItem newPathItem)
        {
            swaggerDoc.Paths.Add(
                ReplaceODataEntityName(odataPathItem.Key, entity.PluralName),
                newPathItem);
        }

        private static string ReplaceODataEntityName(string currentPath, string pluralEntityName)
        {
            return currentPath.Replace($"/{RoutingConstants.ODATA_ROUTE_PREFIX}/{RoutingConstants.EntitySetParameterPathName}", $"/{RoutingConstants.ODATA_ROUTE_PREFIX}/{pluralEntityName}", StringComparison.OrdinalIgnoreCase);
        }

        private OpenApiPathItem CreateACopyOfExistingPathItem(KeyValuePair<string, OpenApiPathItem> odataPathItem)
        {
            return new OpenApiPathItem()
            {
                Servers = odataPathItem.Value.Servers,
                Parameters = odataPathItem.Value.Parameters,
                Extensions = odataPathItem.Value.Extensions,
                Summary = odataPathItem.Value.Summary,
                Description = odataPathItem.Value.Description
            };
        }

        private OpenApiOperation CreateACopyOfExistingOperation(KeyValuePair<OperationType, OpenApiOperation> operation)
        {
            return new OpenApiOperation()
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
