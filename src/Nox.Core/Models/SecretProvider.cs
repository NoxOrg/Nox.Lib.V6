using Nox.Core.Interfaces.Secrets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Core.Models
{
    public class SecretProvider : ISecretProvider
    {
        public string Provider { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
