using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;

namespace Nox.Core.Models
{
    public sealed class LoaderTarget : MetaBase, ILoaderTarget
    {
        public string Entity { get; set; } = string.Empty;
    }
}
