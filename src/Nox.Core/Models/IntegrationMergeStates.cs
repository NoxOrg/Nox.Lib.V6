using System.Collections.Concurrent;

namespace Nox.Core.Models;

public class IntegrationMergeStates : ConcurrentDictionary<string, MergeState>
{
    
}