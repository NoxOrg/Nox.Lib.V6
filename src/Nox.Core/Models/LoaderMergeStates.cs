
using System.Collections.Concurrent;

namespace Nox.Core.Models;

public class LoaderMergeStates : ConcurrentDictionary<string, MergeState>
{
}
