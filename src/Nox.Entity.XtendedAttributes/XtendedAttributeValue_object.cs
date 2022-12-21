
using System.ComponentModel.DataAnnotations;

namespace Nox.Entity.XtendedAttributes;

public sealed class XtendedAttributeValue_object : XtendedAttributeValue
{
    [MaxLength(2048)]
    public string Value { get; set; } = null!; // json
}

