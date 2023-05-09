namespace Nox.Api.OData.Constants
{
    public static class RoutingConstants
    {
        public const string ODATA_ROUTE_PREFIX = "api";
        public const string ODATA_METADATA_ROUTE_PREFIX = "api";

        public const string EntitySetParameterName = "entityset";
        public const string PropertyParameterName = "property";
        public const string NavigationParameterName = "navigation";
        public const string KeyParameterName = "key";

        public const string EntitySetParameterPathName = "{" + EntitySetParameterName + "}";
        public const string PropertyParameterPathName = "{" + PropertyParameterName + "}";
        public const string NavigationParameterPathName = "{" + NavigationParameterName + "}";
        public const string KeyParameterPathName = "{" + KeyParameterName + "}";
    }
}
