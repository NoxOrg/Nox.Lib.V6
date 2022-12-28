namespace Nox.Core.Enumerations;

public static class NoxEventSourceEnumHelper
{
    public static string ToFriendlyName(this NoxEventSourceEnum source)
    {
        switch (source)
        {
            case NoxEventSourceEnum.NoxEventSource_DbContext:
                return "Nox Context";
            case NoxEventSourceEnum.NoxEventSource_EtlLoad:
                return "Etl Load";
            case NoxEventSourceEnum.NoxEventSource_EtlMerge:
                return "Etl Merge";
        }

        return "";
    }
}