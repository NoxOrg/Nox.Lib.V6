using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.ExtendedAttributes
{
    public sealed class XtendedAttributeValue_datetime : XtendedAttributeValue
    {
        public DateTimeOffset? Value { get; set; }
    }
}
