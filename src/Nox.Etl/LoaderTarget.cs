using Nox.Core.Components;
using Nox.Core.Interfaces;

namespace Nox.Etl
{
    public sealed class LoaderTarget : MetaBase, ILoaderTarget
    {
        public string Entity { get; set; } = string.Empty;
    }
}
