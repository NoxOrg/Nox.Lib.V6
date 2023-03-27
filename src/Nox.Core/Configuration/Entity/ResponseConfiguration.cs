using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class ResponseConfiguration : MetaBase
{
    public string ResponseDto { get; set; } = string.Empty;

    public bool IsCollection { get; set; } = false;
}