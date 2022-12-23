namespace Nox.Core.Enumerations;

public static class NoxEventSourceEnumHelper
{
    public static string ToFriendlyName(this NoxEventSourceEnum source)
    {
        switch (source)
        {
            case NoxEventSourceEnum.NoxEventSourceDomain:
                return "Domain";
            case NoxEventSourceEnum.NoxEventSourceEtlLoad:
                return "Etl Load";
            case NoxEventSourceEnum.NoxEventSourceEtlMerge:
                return "Etl Merge";
        }

        return "";
    }
}