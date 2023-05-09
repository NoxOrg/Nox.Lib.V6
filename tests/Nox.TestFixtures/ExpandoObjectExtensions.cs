using System.Collections.Generic;
using System.Dynamic;

namespace Nox.TestFixtures;

public static class ExpandoObjectExtensions
{
    public static void AddProperty(this ExpandoObject source, string propertyName, object propertyValue)
    {
        var dict = source as IDictionary<string, object>;
        if (dict.ContainsKey(propertyName))
        {
            dict[propertyName] = propertyValue;
        }
        else
        {
            dict.Add(propertyName, propertyValue);
        }
    }
}